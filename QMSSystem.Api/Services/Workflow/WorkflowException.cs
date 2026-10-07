namespace QMSSystem.Api.Services.Workflow;

// Thrown when an action breaks a workflow rule, for example raising a deviation
// on a document that is not approved. The API returns it as HTTP 409 with the
// message, so the page can show the message to the user.
public sealed class WorkflowException(string message) : Exception(message)
{
    public static WorkflowException NotFound(string itemType, int id) =>
        new($"{itemType} {id} was not found.");
}
