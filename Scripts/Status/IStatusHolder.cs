/// <summary>
/// Represents an entity that can hold status effects.
/// </summary>
public interface IStatusHolder
{
    /// <summary>
    /// Applies the given status. If a status of the same type is already applied, refreshes that status with given duration.
    /// </summary>
    /// <typeparam name="S">The type of status to apply.</typeparam>
    /// <param name="duration">The duration to apply the status for.</param>
    void ApplyStatus<S>(float duration) where S : Status, new();

    /// <summary>
    /// Returns whether or not the given status is applied.
    /// </summary>
    /// <typeparam name="S">The type of status.</typeparam>
    /// <returns>Whether or not the given status is applied.</returns>
    bool HasStatus<S>() where S : Status;

    /// <summary>
    /// Returns the instance of given status type being applied.
    /// </summary>
    /// <typeparam name="S">The type of status.</typeparam>
    /// <returns>The instance of given status that is being applied.</returns>
    S GetStatusOrNull<S>() where S : Status;

    /// <summary>
    /// Returns a float from 0.0 to 1.0 representing the duration left on the status.
    /// </summary>
    /// <typeparam name="S">The type of status</typeparam>
    /// <returns>The percentage duration left on the status</returns>
    float GetPercentLeft<S>() where S : Status;

    /// <summary>
    /// Called when the status ends. Mainly for IStatusHolders' use.
    /// </summary>
    /// <param name="status">The status that ended</param>
    void OnStatusEnd(Status status);
}
