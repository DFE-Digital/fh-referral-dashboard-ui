
namespace RclTemp.Models;

public enum Phase
{
    Alpha,
    Beta
}

public class PhaseBanner
{
    public Phase Phase { get; }

    public string FeedbackUrl { get; }

    public PhaseBanner(string feedbackUrl, Phase phase = Phase.Beta)
    {
        Phase = phase;
        FeedbackUrl = feedbackUrl;
    }
}