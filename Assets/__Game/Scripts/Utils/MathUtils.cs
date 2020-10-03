using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtils
{
    public static int NumSign(float num)
    {
        if (num < 0) 
            return -1;

        return 1;
    }
}
