using System;

namespace AcademiaDoZe.Application.DTOs;

public class AcessoHistoricoItemDto
{
    public int Id { get; set; }
    public string NomePessoa { get; set; } = string.Empty;
    public string TipoPessoa { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataHora { get; set; }
    public string HoraFormatada => DataHora.ToString("HH:mm:ss");
    public bool IsEntrada { get; set; }
    public ArquivoDto? Foto { get; set; }
}
