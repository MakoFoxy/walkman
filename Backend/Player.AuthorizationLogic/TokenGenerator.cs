using System; // Импорт базового пространства имен .NET для доступа к общим функциям, таким как класс DateTime
using System.Collections.Generic; // Импорт пространства имен для использования коллекций, например List
using System.IdentityModel.Tokens.Jwt; // Импорт пространства имен для работы с JWT (JSON Web Tokens)
using System.Security.Claims; // Импорт пространства имен для работы с утверждениями безопасности (claims)
using System.Text; // Импорт пространства имен для работы с текстом и его кодировками
using Microsoft.IdentityModel.Tokens; // Импорт пространства имен для работы с токенами безопасности Microsoft
using Player.Domain; // Импорт пользовательского пространства имен, связанного с доменом игрока
using Player.Domain.Base; // Импорт базового доменного пространства имен для игрока

namespace Player.AuthorizationLogic // Определение пространства имен для логики авторизации игрока
{
    public class TokenGenerator : ITokenGenerator // Определение класса TokenGenerator, который реализует интерфейс ITokenGenerator
    {
        private readonly AuthOptions _authOptions; // Определение приватного поля для хранения настроек аутентификации

        public TokenGenerator(AuthOptions authOptions) // Конструктор класса, принимающий настройки аутентификации
        //Определяет класс TokenGenerator в пространстве имен Player.AuthorizationLogic. Класс реализует интерфейс ITokenGenerator, что означает, что он предоставляет реализацию всех методов, определенных в этом интерфейсе.
        {
            _authOptions = authOptions; // Присваивание входящих настроек аутентификации внутреннему полю
        }

        public string Generate(User user) // Определение метода Generate, который принимает объект пользователя и возвращает строку токена
                                          //Этот метод принимает объект User, генерирует из него ClaimsIdentity и использует эту идентичность для генерации JWT токена. Если пользователь равен null или не действителен, метод возвращает пустую строку.
        {
            var identity = GetIdentity(user); // Получение объекта ClaimsIdentity на основе предоставленного пользователя

            if (identity == null) // Проверка, была ли успешно создана идентичность пользователя
            {
                return ""; // Возврат пустой строки, если идентичность не создана
            }

            var tokenHandler = new JwtSecurityTokenHandler(); // Создание экземпляра обработчика токенов JWT
            var key = Encoding.ASCII.GetBytes(_authOptions.Key); // Кодирование секретного ключа из настроек в массив байтов
            var tokenDescriptor = new SecurityTokenDescriptor // Создание дескриптора токена с указанием его свойств
            {
                Subject = new ClaimsIdentity(new[] // Назначение идентификации с утверждениями для пользователя
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()) // Утверждение с именем пользователя, использующее его ID
                }),
                Issuer = _authOptions.Issuer, // Назначение издателя токена из настроек
                Audience = _authOptions.Audience, // Назначение аудитории токена из настроек
                Expires = DateTime.UtcNow.AddHours(_authOptions.Lifetime), // Установка времени истечения токена
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature) // Назначение подписывающих учетных данных с использованием секретного ключа и алгоритма HMAC SHA256
            };

            var token = tokenHandler.CreateToken(tokenDescriptor); // Создание токена на основе его описания
            return tokenHandler.WriteToken(token); // Преобразование токена в строку и ее возврат
        }
        //Этот раздел строит JWT токен. Сначала он преобразует ключ аутентификации в байты и настраивает полезную нагрузку и свойства токена, такие как издатель, аудитория, срок действия и подписывающие учетные данные. Затем JwtSecurityTokenHandler создает и возвращает строку токена.
        private ClaimsIdentity GetIdentity(Entity user) // Определение приватного метода GetIdentity для получения идентичности пользователя
        {
            if (user == null) // Проверка на null для предоставленного пользователя
            {
                return null; // Возврат null, если пользователь не предоставлен
            }

            var claims = new List<Claim> // Создание списка утверждений
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Id.ToString()) // Добавление утверждения с именем пользователя, используя его ID
            };
            var claimsIdentity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType); // Создание объекта ClaimsIdentity с использованием списка утверждений
            return claimsIdentity; // Возврат созданной идентичности пользователя
        }
        //Этот частный метод берет Entity (предположительно, сущность пользователя) и преобразует его в ClaimsIdentity. Если пользователь равен null, он возвращает null. В противном случае он создает список утверждений (в данном случае только ID пользователя) и создает из этого списка ClaimsIdentity. Эта идентичность затем используется при генерации JWT токена.

    }
}

