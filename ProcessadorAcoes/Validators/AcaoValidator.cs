using FluentValidation;
using ProcessadorAcoes.Models;

namespace ProcessadorAcoes.Validators
{
    public class AcaoValidator : AbstractValidator<Acao>
    {
        public AcaoValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().WithMessage("Preencha o campo 'Codigo'");

            RuleFor(c => c.Valor).NotEmpty().WithMessage("Preencha o campo 'Valor'")
                .GreaterThan(0).WithMessage("O campo 'Mensagem' deve ser maior do 0");
        }        
    }
}