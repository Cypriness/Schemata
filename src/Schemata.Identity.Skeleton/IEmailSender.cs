using System.Threading.Tasks;
using Schemata.Identity.Skeleton.Entities;

namespace Schemata.Identity.Skeleton;

public interface IEmailSender<in TUser>
    where TUser : SchemataUser
{
    Task SendConfirmationCodeAsync(TUser user, string email, string code);

    Task SendPasswordResetCodeAsync(TUser user, string email, string code);
}
