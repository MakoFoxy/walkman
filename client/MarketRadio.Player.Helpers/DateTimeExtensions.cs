using System;

namespace MarketRadio.Player.Helpers
{
    public static class DateTimeExtensions
    {
        public static DateTimeOffset ResetToSeconds(this DateTimeOffset dateTime)
        {
            return dateTime.AddMilliseconds(-dateTime.Millisecond);
        }
        //Определение метода расширения: Метод ResetToSeconds определен как public static, что является требованием для всех методов расширения. Ключевое слово this перед типом первого параметра (DateTimeOffset dateTime) указывает, что метод расширяет класс DateTimeOffset.Логика метода: Метод вычитает количество миллисекунд, присутствующих в текущем значении dateTime, из этого значения. Это достигается путем использования метода AddMilliseconds с отрицательным значением dateTime.Millisecond. Так как dateTime.Millisecond возвращает количество миллисекунд в текущей секунде, вычитание этого значения обнуляет миллисекунды. Возвращаемое значение: Метод возвращает новый объект DateTimeOffset, где часть миллисекунд установлена в 0, а остальная часть даты и времени остается неизменной. Этот метод расширения может быть полезен, когда требуется работать с точностью до секунд и миллисекунды могут влиять на логику сравнения, сортировки или другие операции с датами и временем.
    }
}