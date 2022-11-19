using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseTest : MonoBehaviour
{
    // float fract(float x)
    // {
    //     return x - Mathf.Floor(x);
    // }
    //
    // Vector3 noised(Vector2 p )
    // {
    //     Vector2 i = new Vector2(Mathf.Floor(p.x), Mathf.Floor( p.y ));
    //     Vector2 f = new Vector2(fract(p.x), fract( p.y ));
    //
    //     Vector2 u = f*f*f*(f*(f*6.0f-15.0f)+10.0f);
    //     Vector2 du = 30.0f*f*f*(f*(f-2.0f)+1.0f);
    //
    //     Vector2 ga = hash( i + Vector2(0.0,0.0) );
    //     Vector2 gb = hash( i + Vector2(1.0,0.0) );
    //     Vector2 gc = hash( i + Vector2(0.0,1.0) );
    //     Vector2 gd = hash( i + Vector2(1.0,1.0) );
    //
    //     float va = dot( ga, f - Vector2(0.0,0.0) );
    //     float vb = dot( gb, f - Vector2(1.0,0.0) );
    //     float vc = dot( gc, f - Vector2(0.0,1.0) );
    //     float vd = dot( gd, f - Vector2(1.0,1.0) );
    //
    //     return Vector3( va + u.x*(vb-va) + u.y*(vc-va) + u.x*u.y*(va-vb-vc+vd),   // value
    //         ga + u.x*(gb-ga) + u.y*(gc-ga) + u.x*u.y*(ga-gb-gc+gd) +  // derivatives
    //         du * (u.yx*(va-vb-vc+vd) + Vector2(vb,vc) - va));
    // }

}
