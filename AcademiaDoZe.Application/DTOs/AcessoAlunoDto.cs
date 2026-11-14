using System;

namespace AcademiaDoZe.Application.DTOs;

public class AcessoAlunoDto
{
    public int Id { get; set; }
    public int AlunoId { get; set; }
    public AlunoDto? Aluno { get; set; }
    public DateTime DataHoraEntrada { get; set; }
    public DateTime? DataHoraSaida { get; set; }
    public bool EstaNaAcademia => !DataHoraSaida.HasValue;
    public string TempoPermanenciaFormatado => DataHoraSaida.HasValue
        ? (DataHoraSaida.Value - DataHoraEntrada).ToString(@"hh\:mm\:ss")
        : (DateTime.Now - DataHoraEntrada).ToString(@"hh\:mm\:ss");
}
