using System;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Player.Domain;
using Player.DTOs;
using Player.Services.Abstractions;
// Объявляет пространство имен для класса MediaPlanExcelReportCreator, организуя его в иерархии Player.Services.
namespace Player.Services
{
    // Объявляет класс MediaPlanMediaPlanExcelReportCreator, который реализует интерфейс IMediaPlanExcelReportCreator.
    public class MediaPlanMediaPlanExcelReportCreator : IMediaPlanExcelReportCreator
    {
        // Постоянные целочисленные значения, представляющие столбцы отчета Excel для разных точек данных.
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

        // Метод Create принимает PlaylistModel и генерирует отчет Excel.
        public byte[] Create(PlaylistModel playlist)
        {
            // Создает новый экземпляр ExcelPackage.
            var template = new ExcelPackage();
            // Устанавливает для свойства DoAdjustDrawings значение true, что позволяет автоматически регулировать размеры чертежа.
            template.DoAdjustDrawings = true;
            // Инициализирует начальную строку для данных.
            var dataBeginRow = 3;
            // Добавляет в книгу новый лист с именем объекта списка воспроизведения.
            var worksheet = template.Workbook.Worksheets.Add(playlist.Object.Name);
            // Настраивает заголовки на листе Excel, используя предоставленные данные модели и статический текст.
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

            // Проходит по каждой уникальной рекламной дорожке в плейлисте и записывает их данные на лист.
            var rowNumber = 2;

            foreach (var trackModel in playlist.Tracks.Where(t => t.TypeCode == TrackType.Advert).DistinctBy(t => t.Name))
            {
                rowNumber++;
                worksheet.Cells[rowNumber, SummaryTitleColumn].Value = trackModel.Name; // Устанавливает название трека в ячейку.
                worksheet.Cells[rowNumber, SummaryValueColumn].Value = playlist.Tracks.Count(model => model.Name == trackModel.Name); // Устанавливает фактическое количество выходов рекламы в ячейку.
                worksheet.Cells[rowNumber, SummaryAdvertLengthColumn].Value = trackModel.Length.ToString(@"hh\:mm\:ss");
            }

            rowNumber += 2; // Пропускаем строку для отступа.

            // Вызываем метод WriteSummaryInfo для заполнения сводной информации о плейлисте.
            WriteSummaryInfo(playlist, worksheet, rowNumber);
            // Вызываем метод WriteMainBody для заполнения основного содержимого отчёта.
            WriteMainBody(playlist, ref dataBeginRow, worksheet);
            // Вызываем метод WriteSummaryFooter для добавления подведения итогов в конец документа.
            WriteSummaryFooter(playlist, ref dataBeginRow, worksheet);
            // TODO AutoFitColumns не работает на линуксе, хотя раньше работал
            // https://github.com/JanKallman/EPPlus/issues/83
            // worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();   
            // Возвращает сгенерированный Excel-документ в виде массива байт.
            return template.GetAsByteArray();
        }

        // Метод WriteSummaryFooter добавляет итоговую информацию в нижнюю часть листа.
        private void WriteSummaryFooter(PlaylistModel playlist, ref int dataBeginRow, ExcelWorksheet worksheet)
        {
            // Добавляем строку со средним значением различий времени окончания рекламы.
            worksheet.Cells[dataBeginRow, StartTimeColumn].Value = "Среднее значение различий по рекламе";
            worksheet.Cells[dataBeginRow, TrackEndDiffColumn].Value = AverageDiff(playlist, TrackType.Advert).ToString(@"hh\:mm\:ss");

            dataBeginRow++; // Переходим к следующей строке.

            // Добавляем строку со средним значением различий времени окончания музыкальных треков.
            worksheet.Cells[dataBeginRow, StartTimeColumn].Value = "Среднее значение различий по музыке";
            worksheet.Cells[dataBeginRow, TrackEndDiffColumn].Value = AverageDiff(playlist, TrackType.Music).ToString(@"hh\:mm\:ss");

            dataBeginRow++; // Переходим к следующей строке.

            // Добавляем строку с общим средним значением различий времени окончания треков.
            worksheet.Cells[dataBeginRow, StartTimeColumn].Value = "Среднее значение различий";
            worksheet.Cells[dataBeginRow, TrackEndDiffColumn].Value = AverageDiff(playlist).ToString(@"hh\:mm\:ss");
        }

        // Метод WriteSummaryInfo добавляет сводные данные о плейлисте.
        private void WriteSummaryInfo(PlaylistModel playlist, ExcelWorksheet worksheet, int rowNumber)
        {
            // Заполняем информацию о общем времени эфира.
            worksheet.Cells[rowNumber, SummaryTitleColumn].Value = "Всего время эфира";
            worksheet.Cells[rowNumber, SummaryValueColumn].Value = playlist.Object.WorkTime.ToString(@"hh\:mm");

            rowNumber++; // Переходим к следующей строке.

            // Заполняем информацию о времени рекламы.
            var advertTime = TimeSpan.FromSeconds(playlist.Tracks.Where(t => t.TypeCode == TrackType.Advert)
                .Sum(t => t.Length.TotalSeconds));
            worksheet.Cells[rowNumber, SummaryTitleColumn].Value = "Время рекламы";
            worksheet.Cells[rowNumber, SummaryValueColumn].Value = advertTime.ToString(@"hh\:mm\:ss");

            rowNumber++; // Переходим к следующей строке.

            // Заполняем информацию о времени музыки.
            var musicTime = TimeSpan.FromSeconds(playlist.Tracks.Where(t => t.TypeCode == TrackType.Music)
                .Sum(t => t.Length.TotalSeconds));
            worksheet.Cells[rowNumber, SummaryTitleColumn].Value = "Время музыки";
            worksheet.Cells[rowNumber, SummaryValueColumn].Value = musicTime.ToString(@"hh\:mm\:ss");

            rowNumber++; // Переходим к следующей строке.

            // Заполняем информацию о проценте рекламы в эфире.
            worksheet.Cells[rowNumber, SummaryTitleColumn].Value = "Процент рекламы в эфире";
            worksheet.Cells[rowNumber, SummaryValueColumn].Value = $"{advertTime.TotalSeconds / playlist.Object.WorkTime.TotalSeconds * 100}%";
        }
        // Метод WriteMainBody добавляет основные данные плейлиста в лист.
        private void WriteMainBody(PlaylistModel playlist, ref int dataBeginRow, ExcelWorksheet worksheet)
        {
            var insertedRowCount = 0; // Счётчик добавленных строк.

            // Проходимся по каждому треку в плейлисте, упорядоченному по времени начала.
            foreach (var track in playlist.Tracks.OrderBy(m => m.StartTime))
            {
                var rowNumber = dataBeginRow + insertedRowCount; // Определяем номер текущей строки.

                // Заполняем данные трека в соответствующие колонки.
                worksheet.Cells[rowNumber, StartTimeColumn].Value = track.StartTime.ToString("T");
                worksheet.Cells[rowNumber, TrackNameColumn].Value = track.Name;
                worksheet.Cells[rowNumber, TrackLengthColumn].Value = track.Length.ToString(@"hh\:mm\:ss");
                worksheet.Cells[rowNumber, PlaningTrackEndColumn].Value = GetPlaningTrackEnd(track).ToString(@"hh\:mm\:ss");
                worksheet.Cells[rowNumber, ActualTrackEndColumn].Value = GetActualTrackEnd(playlist, insertedRowCount).ToString(@"hh\:mm\:ss");
                worksheet.Cells[rowNumber, TrackEndDiffColumn].Value = GetPlaningTrackEnd(track).Subtract(GetActualTrackEnd(playlist, insertedRowCount)).ToString(@"hh\:mm\:ss");

                // Получаем продолжительность блока и если она не нулевая, заполняем соответствующую ячейку.
                var blockLength = GetBlockLength(playlist, insertedRowCount);
                worksheet.Cells[rowNumber, BlockLengthColumn].Value = blockLength == null ? "" : blockLength.Value.ToString(@"hh\:mm\:ss");

                // Устанавливаем фоновый цвет ячеек в зависимости от типа трека.
                worksheet.Cells[rowNumber, StartTimeColumn, rowNumber, BlockLengthColumn].Style.Fill.PatternType = ExcelFillStyle.Solid;
                switch (track.TypeCode)
                {
                    case TrackType.Advert: // Если трек является рекламой, устанавливаем зелёный цвет.
                        worksheet.Cells[rowNumber, StartTimeColumn, rowNumber, BlockLengthColumn].Style.Fill.BackgroundColor.SetColor(1, 155, 226, 155);
                        break;
                    case TrackType.Music: // Если трек музыкальный, устанавливаем жёлтый цвет.
                        worksheet.Cells[rowNumber, StartTimeColumn, rowNumber, BlockLengthColumn].Style.Fill.BackgroundColor.SetColor(1, 225, 231, 140);
                        break;
                    case TrackType.Silent: // Если трек является тишиной (паузой), устанавливаем синий цвет.
                        worksheet.Cells[rowNumber, StartTimeColumn, rowNumber, BlockLengthColumn].Style.Fill.BackgroundColor.SetColor(1, 88, 184, 214);
                        break;
                }

                insertedRowCount++; // Увеличиваем счётчик добавленных строк.
            }

            // Обновляем индекс строки начала данных, добавляя количество вставленных строк.
            dataBeginRow = insertedRowCount + dataBeginRow;
        }

        private TimeSpan? GetBlockLength(PlaylistModel playlist, int trackIndex)
        {
            //Этот метод GetBlockLength в классе MediaPlanMediaPlanExcelReportCreator предназначен для вычисления продолжительности блока треков одного типа (например, рекламы или музыки) в плейлисте. Вот пошаговое объяснение:
            var tracks = playlist.Tracks.Take(trackIndex + 1).ToList(); //    Из всего списка треков в плейлисте берутся все треки до включительно текущего индекса (trackIndex). Это сделано для определения, сколько времени занимает блок треков до текущего момента.

            var currentTrack = tracks.Last(); //    Из полученного выше списка треков берется последний, который и является текущим треком, для которого мы ищем продолжительность блока.

            if (playlist.Tracks.Count - 1 != trackIndex) //    Проверяется, не является ли текущий трек последним в плейлисте.
            {
                var nextTrack = playlist.Tracks[trackIndex + 1]; //    Если текущий трек не последний, берется следующий трек в плейлисте.

                if (nextTrack.TypeCode == currentTrack.TypeCode) //    Проверяется, принадлежит ли следующий трек к тому же типу, что и текущий. Если да, то это означает, что текущий трек не завершает блок, и метод возвращает null, так как продолжительность блока еще не может быть определена.
                {
                    return null;
                }
            }

            var reversedTracks = tracks.ToList(); //    Создается копия списка треков до текущего включительно, которая затем переворачивается. Это делается для удобства последующего поиска блока, начиная с текущего трека и идя назад по списку.
            reversedTracks.Reverse();

            var tracksInBlock = reversedTracks.TakeWhile(t => t.TypeCode == currentTrack.TypeCode).ToList();
            //    Из перевернутого списка выбираются подряд идущие треки, начиная с текущего, которые принадлежат к тому же типу. Это и есть треки текущего блока.
            var blockLength = TimeSpan.FromSeconds(tracksInBlock.Select(t => t.Length).Sum(span => span.TotalSeconds)); //    Вычисляется общая продолжительность блока, суммируя длительности всех треков в блоке.
            return blockLength; //    Возвращается общая продолжительность блока в виде объекта TimeSpan. Если блок состоял из треков одного типа, следующих друг за другом, эта длительность представляет собой сумму их длительностей.
        }

        private TimeSpan AverageDiff(PlaylistModel playlist)
        {
            var allDiffs = playlist.Tracks
                .Select((track, index) => GetPlaningTrackEnd(track).Subtract(GetActualTrackEnd(playlist, index)))
                .Select(diff => diff < TimeSpan.Zero ? TimeSpan.Zero : diff)
                .ToList(); //    Для каждого трека в плейлисте вычисляется разница между планируемым и фактическим временем окончания. Если полученное значение отрицательное (то есть фактическое время окончания было раньше планируемого), оно заменяется на ноль. Все такие разности собираются в список allDiffs.

            return TimeSpan.FromSeconds(allDiffs.Average(d => d.TotalSeconds)); //    Вычисляется среднее значение всех разностей (в секундах), а затем результат преобразуется обратно в TimeSpan. Это среднее отклонение между планируемым и фактическим временем окончания для всех треков в плейлисте.
        }

        private TimeSpan AverageDiff(PlaylistModel playlist, string trackType)
        {
            var allDiffs = new List<TimeSpan>(); //    Создается новый список для хранения разностей времён для треков определённого типа.

            var index = 0;
            foreach (var track in playlist.Tracks) //    Если тип текущего трека (track.TypeCode) совпадает с заданным типом (trackType), вычисляется его временная разница между планируемым и фактическим временем окончания, как и в первом методе. Отрицательные значения также заменяются на ноль. Разность добавляется в список allDiffs.
            {
                if (track.TypeCode == trackType)
                {
                    var diff = GetPlaningTrackEnd(track).Subtract(GetActualTrackEnd(playlist, index));
                    diff = diff < TimeSpan.Zero ? TimeSpan.Zero : diff;
                    allDiffs.Add(diff);
                }

                index++;
            }

            return TimeSpan.FromSeconds(allDiffs.Any() ? allDiffs.Average(d => d.TotalSeconds) : 0); //    Если в списке есть значения (т.е. существуют треки заданного типа), вычисляется их среднее (в секундах), которое затем преобразуется в TimeSpan. Если треков заданного типа нет, возвращается ноль. Это среднее отклонение между планируемым и фактическим временем окончания только для треков заданного типа.
        }

        private TimeSpan GetPlaningTrackEnd(TrackModel track)
        {
            return track.StartTime.TimeOfDay + track.Length; //Время начала трека, преобразованное в формат, который представляет только время дня (без учета даты). track.Length: Продолжительность трека.
            //Логика работы метода проста: он складывает время начала трека и его продолжительность, чтобы получить планируемое время окончания. Например, если трек начинается в 14:00 (и это значение преобразуется в TimeOfDay), и его длительность составляет 30 минут, то планируемое время окончания будет 14:30.
        }

        private TimeSpan GetActualTrackEnd(PlaylistModel playlist, int trackIndex)
        { //    playlist: Объект плейлиста, который содержит список всех треков.
          // trackIndex: Индекс текущего трека в списке плейлиста.
            if (playlist.Tracks.Count - 1 == trackIndex) //Сначала метод проверяет, является ли данный трек последним в плейлисте (if (playlist.Tracks.Count - 1 == trackIndex)). Если да, то в качестве фактического времени окончания трека используется время окончания всего плейлиста (playlist.Object.EndTime).
            {
                return playlist.Object.EndTime;
            }

            return playlist.Tracks[trackIndex + 1].StartTime.TimeOfDay; //Если трек не последний, то в качестве фактического времени окончания берется время начала следующего трека (playlist.Tracks[trackIndex + 1].StartTime.TimeOfDay). Это предполагает, что каждый трек начинается сразу после окончания предыдущего.
            //Таким образом, если трек не последний в списке, его фактическое время окончания будет равно времени начала следующего трека, предполагая непрерывное воспроизведение плейлиста без перерывов между треками.
        }
    }
}
