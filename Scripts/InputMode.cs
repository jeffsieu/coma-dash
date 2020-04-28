#pragma warning disable SA1514 // Element documentation header should be preceded by blank line
/// <summary>
/// An enumerator describing possible input modes.
/// </summary>
#pragma warning restore SA1514 // Element documentation header should be preceded by blank line
public enum InputMode
{
    /// <summary>
    /// User uses a keyboard.
    /// </summary>
    Keyboard,

    /// <summary>
    /// User uses a controller.
    /// </summary>
    Controller,

    /// <summary>
    /// User uses touch input.
    /// </summary>
    Touch,
}
