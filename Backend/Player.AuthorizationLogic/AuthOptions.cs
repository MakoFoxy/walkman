using System.Text; // Используется для работы с текстовыми данными, в частности для конвертации строки в массив байтов.
using Microsoft.Extensions.Configuration; // Используется для доступа к настройкам конфигурации приложения.
using Microsoft.IdentityModel.Tokens; // Используется для работы с токенами безопасности, включая создание ключей безопасности.

namespace Player.AuthorizationLogic // Определение пространства имен, группирует связанные классы и интерфейсы вместе.
{
    public class AuthOptions // Определение класса AuthOptions, который будет содержать настройки для аутентификации.
    {
        public AuthOptions(IConfiguration configuration) // Конструктор класса, принимающий интерфейс IConfiguration.
        {
            Issuer = configuration["Player:Jwt:Issuer"]; // Извлечение издателя (issuer) токена из конфигурации.
            Audience = configuration["Player:Jwt:Audience"]; // Извлечение аудитории (audience) токена из конфигурации.
            Key = configuration["Player:Jwt:Key"]; // Извлечение ключа для подписи токена из конфигурации.
            Lifetime = int.Parse(configuration["Player:Jwt:Lifetime"]); // Извлечение времени жизни токена из конфигурации и преобразование его в тип int.
        }

        public string Issuer { get; } // Свойство только для чтения, возвращающее издателя токена.
        public string Audience { get; } // Свойство только для чтения, возвращающее аудиторию токена.
        public string Key { get; } // Свойство только для чтения, возвращающее ключ для подписи токена.
        public int Lifetime { get; } // Свойство только для чтения, возвращающее время жизни токена в минутах.

        public SymmetricSecurityKey SymmetricSecurityKey => new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Key)); // Свойство, создающее симметричный ключ безопасности из строки Key, преобразуя ее в массив байтов и оборачивая в объект SymmetricSecurityKey.
    }
}

// Класс AuthOptions используется для хранения параметров, необходимых для создания и валидации JWT токенов. Эти параметры включают в себя Issuer (издатель токена), Audience (аудитория токена), Key (ключ для подписи токена) и Lifetime (время жизни токена). Эти значения обычно извлекаются из файла конфигурации приложения. Класс также предоставляет свойство SymmetricSecurityKey, которое используется при подписывании токена для его безопасности.