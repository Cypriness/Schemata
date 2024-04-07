using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Schemata.Identity.Foundation.Models;

namespace Schemata.Identity.Foundation.Controllers;

public partial class AuthenticateController : ControllerBase
{
    [HttpPost(nameof(Forgot))]
    public async Task<IActionResult> Forgot([FromBody] ForgetRequest request) {
        if (!Options.CurrentValue.AllowPasswordReset) {
            return NotFound();
        }

        var user = await GetUserAsync(request.EmailAddress, request.PhoneNumber);
        if (user is null) {
            return Accepted();
        }

        switch (request) {
            case var _ when !string.IsNullOrWhiteSpace(request.EmailAddress)
                         && await UserManager.IsEmailConfirmedAsync(user):
            {
                var code = await UserManager.GeneratePasswordResetTokenAsync(user);
                await MailSender.SendPasswordResetCodeAsync(user, request.EmailAddress, code);
                break;
            }
            case var _ when !string.IsNullOrWhiteSpace(request.PhoneNumber)
                         && await UserManager.IsPhoneNumberConfirmedAsync(user):
            {
                var code = await UserManager.GeneratePasswordResetTokenAsync(user);
                await MessageSender.SendPasswordResetCodeAsync(user, request.PhoneNumber, code);
                break;
            }
        }

        return Accepted();
    }

    [HttpPost(nameof(Reset))]
    public async Task<IActionResult> Reset([FromBody] ResetRequest request) {
        if (!Options.CurrentValue.AllowPasswordReset) {
            return NotFound();
        }

        var user = await GetUserAsync(request.EmailAddress, request.PhoneNumber);
        if (user is null) {
            return BadRequest();
        }

        var confirmed = request switch {
            var _ when !string.IsNullOrWhiteSpace(request.EmailAddress) => await UserManager.IsEmailConfirmedAsync(user),
            var _ when !string.IsNullOrWhiteSpace(request.PhoneNumber) => await UserManager.IsPhoneNumberConfirmedAsync(user),
            var _ => false,
        };

        if (!confirmed) {
            return BadRequest();
        }

        var result = await UserManager.ResetPasswordAsync(user, request.Code, request.Password);
        if (!result.Succeeded) {
            return BadRequest(result.Errors);
        }

        return NoContent();
    }
}