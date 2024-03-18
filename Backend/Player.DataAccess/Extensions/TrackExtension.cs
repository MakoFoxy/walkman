using System;
using System.Linq;
using Player.Domain;

namespace Player.DataAccess.Extensions
{
    public static class TrackExtension
    {
        public static IQueryable<Advert> GetValidTracksOn(this PlayerContext context, DateTime date)
        {
            return context.Adverts
                .Where(a => a.AdLifetimes.Any(al => al.DateBegin <= date && al.DateEnd >= date && !al.InArchive) && a.IsValid);
        }
    }
}

// класс TrackExtension:

//     Это статический класс, который содержит методы расширения. В C#, методы расширения должны быть определены в статическом классе.

// Метод GetValidTracksOn:

//     Является методом расширения для класса PlayerContext, что указывается ключевым словом this перед первым аргументом метода.
//     Принимает два параметра: PlayerContext context и DateTime date. context — это текущий экземпляр контекста базы данных, а date — дата, на которую нужно найти активные треки (в данном случае объявления).
//     Возвращает объект IQueryable<Advert>, который представляет собой запрос, выбирающий объявления (Adverts), соответствующие заданным условиям:
//         a.AdLifetimes.Any(al => al.DateBegin <= date && al.DateEnd >= date && !al.InArchive): выбираются объявления, для которых существует хотя бы один срок действия (AdLifetime), такой что указанная дата попадает в интервал между DateBegin и DateEnd, и этот срок действия не помечен как архивный (InArchive).
//         a.IsValid: выбираются только объявления, помеченные как валидные (IsValid).

// Использование:

// Этот метод можно использовать, чтобы получить все действующие и валидные объявления для заданной даты из контекста PlayerContext. Это может быть полезно для вывода рекламных объявлений, которые активны в определенный день, без включения объявлений, которые уже устарели или были перемещены в архив.

// Например:

// csharp

// var validAdsOnDate = dbContext.GetValidTracksOn(DateTime.Today).ToList();

// Этот код получит все валидные объявления для текущей даты из контекста данных dbContext.