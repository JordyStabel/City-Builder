//===================================================================
//                  Created by Jordy Stabèl 2018
//            https://github.com/JordyStabel/City-Builder
//===================================================================

using UnityEngine;

public static class InstalledObjectActions {

	public static void Door_UpdateAction(InstalledObject installedObject, float deltaTime)
    {
        // If the door isOpening is 'true' open the door a little bit more
        if (installedObject.GetParameter("isOpening") >= 1)
        {
            installedObject.ChangeParameter("OpenValue", (deltaTime * 4));

            // If door is fully opened, close it again (right away)
            if (installedObject.GetParameter("OpenValue") >= 1)
                installedObject.SetParameter("isOpening", 0);
        }
        // Close door again
        else
            installedObject.ChangeParameter("OpenValue", (deltaTime * -4));

        // Clamp value between 0 & 1
        installedObject.SetParameter("OpenValue", Mathf.Clamp01(installedObject.GetParameter("OpenValue")));

        // Call the callback if there is any
        if (installedObject.cb_OnChanged != null)
            installedObject.cb_OnChanged(installedObject);
    }

    public static EnterAbility Door_IsEnterable(InstalledObject installedObject)
    {
        // Door 'isOpening' = 1, means door is opening = true
        installedObject.SetParameter("isOpening", 1);

        // If door is fully open, character can enter.
        if (installedObject.GetParameter("OpenValue") >= 1)
            return EnterAbility.Yes;

        // Soon, door is going to open soonTM
        return EnterAbility.Soon;
    }
}
