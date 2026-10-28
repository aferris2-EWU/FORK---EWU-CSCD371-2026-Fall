// Combine permissions with the bitwise OR operator. Each permission is represented
// by its own bit, so this value contains both Read and Write.
Permission permissions = Permission.Read | Permission.Write;

// HasFlag checks whether the Read bit is present (true). Equality checks whether
// the whole value is exactly Read (false, because Write is also set).
Console.WriteLine($"{permissions.HasFlag(Permission.Read)}, {permissions == Permission.Read}");

// Flags marks this enum as a set of combinable bit values. The values below are
// powers of two so each permission occupies a separate bit.
[Flags]
public enum Permission
{
    None = 0,    // No permission bits are set.
    Read = 1,    // 001 in binary.
    Write = 2,   // 010 in binary.
    Execute = 4  // 100 in binary.
}
