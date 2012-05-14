using System;

namespace Ziyi
{
    [Flags]
    public enum ShiftState : int
    {
        None = 0,
        LShft = 1,
        RShft = 2,
        LCtrl = 4,
        RCtrl = 8,
        LMenu = 16,
        RMenu = 32,
    }
}
