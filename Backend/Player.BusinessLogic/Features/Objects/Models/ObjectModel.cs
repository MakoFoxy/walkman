using System;
using Player.DTOs;

namespace Player.BusinessLogic.Features.Objects.Models
{
    public class ObjectModel : SimpleDto
    {
        public string ActualAddress { get; set; }
        public TimeSpan BeginTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public SimpleDto FirstPerson { get; set; }
        public double Loading { get; set; }
        public bool IsOnline { get; set; }
        public bool PlaylistExist { get; set; }

        public TimeSpan WorkTime
        {
            get
            {
                var workTime = EndTime.Subtract(BeginTime);

                if (workTime <= TimeSpan.Zero)
                    workTime = workTime.Add(TimeSpan.FromHours(24));

                return workTime;
            }
            // Это свойство вычисляет продолжительность рабочего времени объекта на основе его BeginTime и EndTime. Если результат вычисления отрицательный (что может случиться, если рабочий день переходит через полночь), к продолжительности добавляется 24 часа, чтобы корректно рассчитать интервал.
        }

        public int UniqueAdvertCount { get; set; }
        public int AllAdvertCount { get; set; }
        public bool Overloaded { get; set; }
        
        //     Свойства ObjectModel:

        // ActualAddress: строковое свойство для хранения фактического адреса объекта.
        // BeginTime и EndTime: представляют начало и конец рабочего времени объекта в виде значений TimeSpan.
        // FirstPerson: экземпляр SimpleDto, который может представлять контактное лицо или ответственное за объект лицо.
        // Loading: числовое значение, представляющее уровень загрузки объекта (например, в процентах или другой метрике).
        // IsOnline: булево значение, указывающее, доступен ли объект в онлайн-режиме.
        // PlaylistExist: булево значение, показывающее, существует ли плейлист для данного объекта на определенный день.
        // UniqueAdvertCount и AllAdvertCount: целочисленные значения, представляющие количество уникальных и общих рекламных объявлений в объекте.
        // Overloaded: булево значение, указывающее, превышена ли допустимая нагрузка на объект.
    }
    // Этот класс ObjectModel может использоваться в различных частях системы, где необходимо работать с подробной информацией о конкретных объектах, включая их расписание, загрузку и связанные рекламные материалы.
}