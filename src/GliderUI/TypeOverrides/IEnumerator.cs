using RpcUIShell.Generator;

namespace GliderUI.System.Collections.Generic;

#pragma warning disable CA1063 // Implement IDisposable Correctly
#pragma warning disable CA1707 // Identifiers should not contain underscores
public partial class IEnumerator_Impl<T>
#pragma warning restore CA1707
#pragma warning restore CA1063
{
    // PowerShell calls the non-generic version of Current when it enumerates objects.
    // When a private type is returned, since the return type is object and there is no type mapping avalable, it throws.
    // Always use the typed version of Current so that it can create the corresponding type.
    [SurpressGeneratorPropertyByName]
    object global::System.Collections.IEnumerator.Current
    {
        get => Current!;
    }
}
