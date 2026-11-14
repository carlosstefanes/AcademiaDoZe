using System;

namespace AcademiaDoZe.Application.DTOs;

public class AcessoColaboradorDto
{
    public int Id { get; set; }
    public int ColaboradorId { get; set; }
    public ColaboradorDto? Colaborador { get; set; }
    public DateTime DataHoraEntrada { get; set; }
    public DateTime? DataHoraSaida { get; set; }
    public bool EstaNaAcademia => !DataHoraSaida.HasValue;
    public string TempoPermanenciaFormatado => DataHoraSaida.HasValue
        ? (DataHoraSaida.Value - DataHoraEntrada).ToString(@"hh\:mm\:ss")
        : (DateTime.Now - DataHoraEntrada).ToString(@"hh\:mm\:ss");
}
