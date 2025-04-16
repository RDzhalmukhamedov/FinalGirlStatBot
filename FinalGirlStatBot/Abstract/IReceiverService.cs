namespace FinalGirlStatBot.Abstract;

public interface IReceiverService
{
    Task Receive(CancellationToken stoppingToken);
}
