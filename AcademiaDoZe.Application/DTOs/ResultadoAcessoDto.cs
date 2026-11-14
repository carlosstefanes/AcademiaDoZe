using System;

namespace AcademiaDoZe.Application.DTOs;

public class ResultadoAcessoDto
{
    public bool Sucesso { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string NomePessoa { get; set; } = string.Empty;
    public string TipoPessoa { get; set; } = string.Empty;
    public bool EstaNaAcademia { get; set; }
}
