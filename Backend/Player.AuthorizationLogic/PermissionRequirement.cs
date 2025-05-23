﻿using Microsoft.AspNetCore.Authorization; // Импорт пространства имен, предоставляющего функции для работы с авторизацией в ASP.NET Core.

namespace Player.AuthorizationLogic // Определение пространства имен для организации логической структуры кода, связанного с логикой авторизации в приложении Player.
{
    public class PermissionRequirement : IAuthorizationRequirement // Объявление класса PermissionRequirement, который реализует интерфейс IAuthorizationRequirement, предоставляемый ASP.NET Core для определения требований авторизации.
    {
        public PermissionRequirement(string permission) // Конструктор класса с одним параметром - строкой permission. Это позволяет создавать экземпляр требования с определенным разрешением.
        {
            Permission = permission; // Присвоение значения параметра permission свойству Permission класса. Это значение будет использоваться для проверки прав пользователя.
        }

        public string Permission { get; } // Объявление свойства Permission только для чтения. Это свойство хранит информацию о разрешении, которое требуется для выполнения определенного действия в приложении.
    }
}
// В этом коде определяется класс PermissionRequirement, который используется для представления конкретного требования к разрешениям в системе авторизации. Класс наследует интерфейс IAuthorizationRequirement, что делает его подходящим для использования с ASP.NET Core системой авторизации. Свойство Permission позволяет хранить конкретное разрешение, которое затем может быть проверено в обработчике авторизации, таком как PermissionHandler, который мы разбирали ранее.