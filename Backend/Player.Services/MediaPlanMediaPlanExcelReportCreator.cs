using System;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Player.Domain;
using Player.DTOs;
using Player.Services.Abstractions;

namespace Player.Services
{
    public class MediaPlanMediaPlanExcelReportCreator : IMediaPlanExcelReportCreator
    {
        private const int StartTimeColumn = 1;
        private const int TrackNameColumn = 2;
        private const int TrackLengthColumn = 3;
        private const int PlaningTrackEndColumn = 4;
        private const int ActualTrackEndColumn = 5;
        private const int TrackEndDiffColumn = 6;
        private const int BlockLengthColumn = 7;
        private const int SummaryTitleColumn = 10;
        private const int SummaryValueColumn = 11;
        private const int SummaryAdvertLengthColumn = 12;
        
        public byte[] Create(PlaylistModel playlist)
        {
            var template = new ExcelPackage();
            template.DoAdjustDrawings = true;

            var dataBeginRow = 3;
            
            var worksheet = template.Workbook.Worksheets.Add(playlist.Object.Name);

            worksheet.Cells[1, StartTimeColumn].Value = $"Объект {playlist.Object.Name}";
            worksheet.Cells[1, TrackNameColumn].Value = $"Медиаплан за: {playlist.PlayDate:dd.MM.yyyy}";

            worksheet.Cells[2, StartTimeColumn].Value = "Время выхода";
            worksheet.Cells[2, TrackNameColumn].Value = "Название трека";
            worksheet.Cells[2, TrackLengthColumn].Value = "Длина трека";
            worksheet.Cells[2, PlaningTrackEndColumn].Value = "План. окончание трека";
            worksheet.Cells[2, ActualTrackEndColumn].Value = "Факт. окончание трека";
            worksheet.Cells[2, TrackEndDiffColumn].Value = "Различие";
            worksheet.Cells[2, BlockLengthColumn].Value = "Продолжительность блока";
            
            worksheet.Cells[2, SummaryTitleColumn].Value = "Список рекламных кампаний";
            worksheet.Cells[2, SummaryValueColumn].Value = "Фактическое кол-во выходов";
            worksheet.Cells[2, SummaryAdvertLengthColumn].Value = "Длина рекламы";
            
            var rowNumber = 2;
            
            foreach (var trackModel in playlist.Tracks.Where(t => t.TypeCode == TrackType.Advert).DistinctBy(t => t.Name))
            {
                rowNumber++;
                worksheet.Cells[rowNumber, SummaryTitleColumn].Value = trackModel.Name;
                worksheet.Cells[rowNumber, SummaryValueColumn].Value = playlist.Tracks.Count(model => model.Name == trackModel.Name);
                worksheet.Cells[rowNumber, SummaryAdvertLengthColumn].Value = trackModel.Length.ToString(@"hh\:mm\:ss");
            }

            rowNumber += 2;
            
            WriteSummaryInfo(playlist, worksheet, rowNumber);

            WriteMainBody(playlist, ref dataBeginRow, worksheet);
            WriteSummaryFooter(playlist, ref dataBeginRow, worksheet);
            // TODO AutoFitColumns не работает на линуксе, хотя раньше работал
            // https://github.com/JanKallman/EPPlus/issues/83
            // worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();   

            return template.GetAsByteArray();
        }

        private void WriteSummaryFooter(PlaylistModel playlist, ref int dataBeginRow, ExcelWorksheet worksheet)
        {
            // TODO сделать
            worksheet.Cells[dataBeginRow, StartTimeColumn].Value = "Среднее значение различий по рекламе";
            worksheet.Cells[dataBeginRow, TrackEndDiffColumn].Value = AverageDiff(playlist, TrackType.Advert).ToString(@"hh\:mm\:ss");
            
            dataBeginRow++;
            
            // TODO сделать
            worksheet.Cells[dataBeginRow, StartTimeColumn].Value = "Среднее значение различий по музыке";
            worksheet.Cells[dataBeginRow, TrackEndDiffColumn].Value = AverageDiff(playlist, TrackType.Music).ToString(@"hh\:mm\:ss");
            
            dataBeginRow++;
            
            worksheet.Cells[dataBeginRow, StartTimeColumn].Value = "Среднее значение различий";
            worksheet.Cells[dataBeginRow, TrackEndDiffColumn].Value = AverageDiff(playlist).ToString(@"hh\:mm\:ss");
        }

        private void WriteSummaryInfo(PlaylistModel playlist, ExcelWorksheet worksheet, int rowNumber)
        {
            worksheet.Cells[rowNumber, SummaryTitleColumn].Value = "Всего время эфира";
            worksheet.Cells[rowNumber, SummaryValueColumn].Value = playlist.Object.WorkTime.ToString(@"hh\:mm");

            rowNumber++;
            var advertTime = TimeSpan.FromSeconds(playlist.Tracks.Where(t => t.TypeCode == TrackType.Advert)
                .Sum(t => t.Length.TotalSeconds));

            worksheet.Cells[rowNumber, SummaryTitleColumn].Value = "Время рекламы";
            worksheet.Cells[rowNumber, SummaryValueColumn].Value = advertTime.ToString(@"hh\:mm\:ss");

            rowNumber++;
            var musicTime = TimeSpan.FromSeconds(playlist.Tracks.Where(t => t.TypeCode == TrackType.Music)
                .Sum(t => t.Length.TotalSeconds));

            worksheet.Cells[rowNumber, SummaryTitleColumn].Value = "Время музыки";
            worksheet.Cells[rowNumber, SummaryValueColumn].Value = musicTime.ToString(@"hh\:mm\:ss");

            rowNumber++;
            worksheet.Cells[rowNumber, SummaryTitleColumn].Value = "Процент рекламы в эфире";
            worksheet.Cells[rowNumber, SummaryValueColumn].Value = $"{advertTime.TotalSeconds / playlist.Object.WorkTime.TotalSeconds * 100}%";
        }

        private void WriteMainBody(PlaylistModel playlist, ref int dataBeginRow, ExcelWorksheet worksheet)
        {
            var insertedRowCount = 0;
            foreach (var track in playlist.Tracks.OrderBy(m => m.StartTime))
            {
                var rowNumber = dataBeginRow + insertedRowCount;

                worksheet.Cells[rowNumber, StartTimeColumn].Value = track.StartTime.ToString("T");
                worksheet.Cells[rowNumber, TrackNameColumn].Value = track.Name;
                worksheet.Cells[rowNumber, TrackLengthColumn].Value = track.Length.ToString(@"hh\:mm\:ss");
                worksheet.Cells[rowNumber, PlaningTrackEndColumn].Value = GetPlaningTrackEnd(track).ToString(@"hh\:mm\:ss");
                worksheet.Cells[rowNumber, ActualTrackEndColumn].Value = GetActualTrackEnd(playlist, insertedRowCount).ToString(@"hh\:mm\:ss");
                worksheet.Cells[rowNumber, TrackEndDiffColumn].Value = GetPlaningTrackEnd(track).Subtract(GetActualTrackEnd(playlist, insertedRowCount)).ToString(@"hh\:mm\:ss");
                var blockLength = GetBlockLength(playlist, insertedRowCount);
                worksheet.Cells[rowNumber, BlockLengthColumn].Value = blockLength == null ? "" : blockLength.Value.ToString(@"hh\:mm\:ss");

                worksheet.Cells[rowNumber, StartTimeColumn, rowNumber, BlockLengthColumn].Style.Fill.PatternType = ExcelFillStyle.Solid;

                switch (track.TypeCode)
                {
                    case TrackType.Advert:
                        worksheet.Cells[rowNumber, StartTimeColumn, rowNumber, BlockLengthColumn].Style.Fill.BackgroundColor.SetColor(1, 155, 226, 155);
                        break;
                    case TrackType.Music:
                        worksheet.Cells[rowNumber, StartTimeColumn, rowNumber, BlockLengthColumn].Style.Fill.BackgroundColor.SetColor(1, 225, 231, 140);
                        break;
                    case TrackType.Silent:
                        worksheet.Cells[rowNumber, StartTimeColumn, rowNumber, BlockLengthColumn].Style.Fill.BackgroundColor.SetColor(1, 88, 184, 214);
                        break;
                }

                insertedRowCount++;
            }

            dataBeginRow = insertedRowCount + dataBeginRow;
        }

        private TimeSpan? GetBlockLength(PlaylistModel playlist, int trackIndex)
        {
            var tracks = playlist.Tracks.Take(trackIndex + 1).ToList();

            var currentTrack = tracks.Last();

            if (playlist.Tracks.Count - 1 != trackIndex)
            {
                var nextTrack = playlist.Tracks[trackIndex + 1];

                if (nextTrack.TypeCode == currentTrack.TypeCode)
                {
                    return null;
                }
            }

            var reversedTracks = tracks.ToList();
            reversedTracks.Reverse();

            var tracksInBlock = reversedTracks.TakeWhile(t => t.TypeCode == currentTrack.TypeCode).ToList();

            var blockLength = TimeSpan.FromSeconds(tracksInBlock.Select(t => t.Length).Sum(span => span.TotalSeconds));
            return blockLength;
        }

        private TimeSpan AverageDiff(PlaylistModel playlist)
        {
            var allDiffs = playlist.Tracks
                .Select((track, index) => GetPlaningTrackEnd(track).Subtract(GetActualTrackEnd(playlist, index)))
                .Select(diff => diff < TimeSpan.Zero ? TimeSpan.Zero : diff)
                .ToList();

            return TimeSpan.FromSeconds(allDiffs.Average(d => d.TotalSeconds));
        }

        private TimeSpan AverageDiff(PlaylistModel playlist, string trackType)
        {
            var allDiffs = new List<TimeSpan>();

            var index = 0;
            foreach (var track in playlist.Tracks)
            {
                if (track.TypeCode == trackType)
                {
                    var diff = GetPlaningTrackEnd(track).Subtract(GetActualTrackEnd(playlist, index));
                    diff = diff < TimeSpan.Zero ? TimeSpan.Zero : diff;
                    allDiffs.Add(diff);
                }

                index++;
            }

            return TimeSpan.FromSeconds(allDiffs.Any() ? allDiffs.Average(d => d.TotalSeconds) : 0);
        }
        
        private TimeSpan GetPlaningTrackEnd(TrackModel track)
        {
            return track.StartTime.TimeOfDay + track.Length;
        }
        
        private TimeSpan GetActualTrackEnd(PlaylistModel playlist, int trackIndex)
        {
            if (playlist.Tracks.Count - 1 == trackIndex)
            {
                return playlist.Object.EndTime;
            }
            
            return playlist.Tracks[trackIndex + 1].StartTime.TimeOfDay;
        }
    }
}
