﻿using System;

namespace Player.DTOs
{
    public class SimpleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        // public object SomeNumericField { get; set; }
        // public bool IsSelected { get; set; } // флаг, указывающее на выбор пользователя
    }
}
