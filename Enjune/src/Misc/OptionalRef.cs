using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

// Если вы когда-нибудь почувствуете себя хуесосом то вспомните ref
public readonly ref struct OptionalRef<T>
{
    private readonly Span<T> _storage;

    public OptionalRef(ref T value)
    {
        _storage = MemoryMarshal.CreateSpan(ref value, 1);
    }

    public bool HasValue => !_storage.IsEmpty;
    public ref T Value => ref _storage[0];
}
