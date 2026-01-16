using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Legend:
// u = user
// m = monster
// k = kitty
// w = weapon
// * = hole
// = = wall
//
// Constraints:
// Level should have one user, one weapon, 
// at least one kitty, and at least one monster.

public static class Levels
{
    public static readonly List<string[]> All = new()
    {
        // level1
        new[]
        {
            "w=      ",
            " =  k ==",
            "        ",
            "==      ",
            "        ",
            "       =",
            "   =    ",
            "u  =   m"
        },

        // level2
        new[]
        {
            "        ",
            "  *   k ",
            "w       ",
            "    m * ",
            "        ",
            "   *    ",
            " =======",
            "       u"
        },

        // level3
        new[]
        {
            "        ",
            " === == ",
            " =    = ",
            " =w   = ",
            "k=    =u",
            " =m   = ",
            " == === ",
            "        "
        },

        // level4
        new[]
        {
            "u     =w",
            "    m = ",
            "      = ",
            "      = ",
            "      = ",
            "     k= ",
            " ====== ",
            "        ",
        },

        // level5
        new[]
        {
            "       u",
            " =====  ",
            " =   =  ",
            " = = =  ",
            " = =w= m",
            " = ===  ",
            " =      ",
            "   k   =",
        },

        // level6
        new[]
        {
            "========",
            "=*u m *=",
            "=  *   =",
            "= ***  =",
            "= **** =",
            "=   *  =",
            "=*k  w*=",
            "========",
        },

        // level7
        new[]
        {
            "========",
            "========",
            "=m    u=",
            "= *=*  =",
            "= =*=  =",
            "=w    k=",
            "========",
            "========",
        },

        // level8
        new[]
        {
            "========",
            "========",
            "==u  *==",
            "== k  ==",
            "==  * ==",
            "==  mw==",
            "========",
            "========",
        },

        // level9
        new[]
        {
            "        ",
            " ====== ",
            " =   w= ",
            " = == = ",
            " =    = ",
            " =   m= ",
            " == === ",
            " k    u "
        },

        // level10
        new[]
        {
            "  w* *  ",
            " *    * ",
            " m  *   ",
            "   *  k ",
            "  *    *",
            "*    *  ",
            " *  * * ",
            "*   u  *",
        },
    };
}
