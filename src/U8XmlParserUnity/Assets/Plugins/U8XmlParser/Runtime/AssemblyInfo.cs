#if UNITY_2018_1_OR_NEWER
#define IS_UNITY
#endif

#if !IS_UNITY
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("UnitTest")]
#endif
