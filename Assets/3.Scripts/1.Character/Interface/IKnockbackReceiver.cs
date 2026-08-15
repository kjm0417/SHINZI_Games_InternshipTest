using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IKnockbackReceiver
{
    void ApplyKnockback(Vector3 direction, float power);
}
