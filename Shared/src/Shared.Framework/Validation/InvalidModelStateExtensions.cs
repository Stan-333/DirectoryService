using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core.Http;
using Shared.Kernel;

namespace Shared.Framework.Validation;

/// <summary>
/// Ответы MVC на ошибки привязки и валидации модели в формате <see cref="Envelope"/>.
/// </summary>
public static class InvalidModelStateExtensions
{
    private const string InvalidValueCode = "value.is.invalid";
    private const string InvalidFormatMessage = "Значение имеет неверный формат";

    /// <summary>
    /// Заменяет стандартный ответ <c>[ApiController]</c> на невалидную модель (ProblemDetails)
    /// ответом 400 с <see cref="Envelope"/>: так клиент получает ошибки привязки (битый JSON,
    /// не заполненное обязательное поле, неверный формат значения) в том же формате, что и ошибки хендлеров.
    /// </summary>
    /// <param name="builder">Построитель MVC, который возвращает <c>AddControllers()</c>.</param>
    /// <returns>Тот же построитель.</returns>
    public static IMvcBuilder AddEnvelopeValidationResponses(this IMvcBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ConfigureApiBehaviorOptions(options =>
            options.InvalidModelStateResponseFactory = context =>
                new ObjectResult(Envelope.Error(context.ModelState.ToErrors()))
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ContentTypes = { "application/json" },
                });

        return builder;
    }

    /// <summary>
    /// Превращает ошибки <see cref="ModelStateDictionary"/> в <see cref="Errors"/>: каждая ошибка —
    /// <see cref="ErrorType.Validation"/> с кодом <c>value.is.invalid</c> и именем поля в <see cref="Error.InvalidField"/>.
    /// Текст исключения, из-за которого не удалась привязка, клиенту не передаётся.
    /// </summary>
    /// <param name="modelState">Состояние модели.</param>
    /// <returns>Ошибки валидации; пустой список, если ошибок нет.</returns>
    public static Errors ToErrors(this ModelStateDictionary modelState)
    {
        ArgumentNullException.ThrowIfNull(modelState);

        List<Error> errors = [];
        foreach ((string key, ModelStateEntry? entry) in modelState)
        {
            if (entry is null)
            {
                continue;
            }

            string? field = string.IsNullOrEmpty(key) ? null : key;
            foreach (ModelError modelError in entry.Errors)
            {
                string message = string.IsNullOrWhiteSpace(modelError.ErrorMessage)
                    ? InvalidFormatMessage
                    : modelError.ErrorMessage;

                errors.Add(Error.Validation(InvalidValueCode, message, field));
            }
        }

        return errors;
    }
}