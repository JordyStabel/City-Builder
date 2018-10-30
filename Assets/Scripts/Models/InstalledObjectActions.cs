//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;

public static class InstalledObjectActions {

	public static void Door_UpdateAction(InstalledObject installedObject, float deltaTime)
    {
        // If the door isOpening is 'true' open the door a little bit more
        if (installedObject.installedObjectParameters["isOpening"] >= 1)
        {
            installedObject.installedObjectParameters["OpenValue"] += deltaTime;

            // If door is fully opened, close it again (right away)
            if (installedObject.installedObjectParameters["OpenValue"] >= 1)
                installedObject.installedObjectParameters["isOpening"] = 0;
        }
        // Close door again
        else
            installedObject.installedObjectParameters["OpenValue"] -= deltaTime;

        // Clamp value between 0 & 1
        installedObject.installedObjectParameters["OpenValue"] = Mathf.Clamp01(installedObject.installedObjectParameters["OpenValue"]);
    }

    public static EnterAbility Door_IsEnterable(InstalledObject installedObject)
    {
        // Door 'isOpening' = 1, means door is opening = true
        installedObject.installedObjectParameters["isOpening"] = 1;

        // If door is fully open, character can enter.
        if (installedObject.installedObjectParameters["OpenValue"] >= 1)
            return EnterAbility.Yes;

        // Soon, door is going to open soonTM
        return EnterAbility.Soon;
    }
}
