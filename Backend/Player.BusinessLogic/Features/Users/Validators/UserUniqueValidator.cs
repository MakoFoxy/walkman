using System;
using System.Linq;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.Domain;

namespace Player.BusinessLogic.Features.Users.Validators
{
    public class UserUniqueValidator : AbstractValidator<User>
    {
        public UserUniqueValidator(PlayerContext context)
        {
            RuleFor(u => u)
                .Must(user =>
                {
                    if (user.Id != Guid.Empty)
                    {
                        // TODO Это надо разобрать по хорошему 
                        return true;
                    }
                    
                    var exists = context.Users.Any(u => u.Email.ToLower() == user.Email.ToLower());
                    return !exists;
                })
                .WithMessage(user => $"Пользователь с почтой {user.Email} уже существует");
        }
    }
}