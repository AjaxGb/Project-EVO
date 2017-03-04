/*/
* Script by Devin Curry
* www.Devination.com
* www.youtube.com/user/curryboy001
* Please like and subscribe if you found my tutorials helpful :D
* Google+ Community: https://plus.google.com/communities/108584850180626452949
* Twitter: https://twitter.com/Devination3D
* Facebook Page: https://www.facebook.com/unity3Dtutorialsbydevin
/*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vec2toVec3ExtensionMethod {

    public static Vector2 toVector2(this Vector3 vec3)
    {
        return new Vector2(vec3.x, vec3.y);
    }

}
