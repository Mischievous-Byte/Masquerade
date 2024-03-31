using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Masquerade
{
    public delegate void RemapDelegate(
            in BodyTree<Matrix4x4> source,
            ref BodyTree<Matrix4x4> destination);

    public delegate void RemapDelegate<TSettings>(
        in BodyTree<Matrix4x4> source,
        ref BodyTree<Matrix4x4> destination,
        in TSettings data);

    public delegate void RemapDelegate<TUniqueS, TUniqueD>(
        in BodyTree<Matrix4x4> source,
        in TUniqueS sourceData,
        ref BodyTree<Matrix4x4> destination,
        in TUniqueD destinationData);

    public delegate void RemapDelegate<TUniqueS, TUniqueD, TSettings>(
        in BodyTree<Matrix4x4> source,
        in TUniqueS sourceData,
        ref BodyTree<Matrix4x4> destination,
        in TUniqueD destinationData,
        in TSettings data);
}
