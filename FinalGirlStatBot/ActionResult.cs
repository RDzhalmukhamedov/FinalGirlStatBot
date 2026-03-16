namespace FinalGirlStatBot;

public class ActionResult
{
    public bool Success { get; set; }

    public GameState? TargetState { get; set; }

    public string AdditionalMessage { get; set; } = "";

    public static ActionResult Ok(GameState? targetState = null, string additionalMessage = "")
    {
        return new ActionResult
        {
            Success = true,
            AdditionalMessage = additionalMessage,
            TargetState = targetState
        };
    }

    public static ActionResult Error(string additionalMessage = "")
    {
        return new ActionResult
        {
            Success = false,
            AdditionalMessage = additionalMessage
        };
    }

}
