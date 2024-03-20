using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Player.Helpers
{
    public static class TypeUtilities
    {
        public static List<T> GetAllPublicConstantValues<T>(this Type type)
        {
            return type
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy) //GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy): Этот вызов получает все публичные статические поля этого типа, включая поля из базовых классов (за счет флага BindingFlags.FlattenHierarchy).
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(T)) //.Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(T)): Фильтрует поля так, чтобы остались только те, которые являются литералами (константами), не являются только для инициализации (что исключает static readonly поля), и тип которых соответствует типу T.
                .Select(x => (T)x.GetRawConstantValue()) //.Select(x => (T)x.GetRawConstantValue()): Преобразует каждое поле в его значение и кастует его к типу T. Метод GetRawConstantValue() используется для получения значения константного поля.
                .ToList(); //.ToList(): Преобразует итерируемую коллекцию в List<T>.

            //Метод расширения GetAllPublicConstantValues<T> может быть очень полезен, когда вам нужно получить все значения определенного типа, которые определены как публичные константы в типе. Это может пригодиться, например, при реализации настроек, конфигурации, маппингов или когда необходимо получить список всех поддерживаемых значений для определенных типов данных.
        }
    }
}

// метод GetAllPublicConstantValues<T> в классе TypeUtilities — это утилита, предназначенная для получения всех публичных констант заданного типа T из указанного класса или типа.

// Этот метод использует рефлексию для динамического сканирования полей в классе и извлекает те, которые являются публичными, статическими константами, и соответствуют заданному типу T. Это может быть особенно полезно для извлечения конфигурационных значений, перечислений, идентификаторов ресурсов и других элементов, которые обычно задаются как константы в программном коде.