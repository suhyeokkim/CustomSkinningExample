/* dqconv.c

  Conversion routines between (regular quaternion, translation) and dual quaternion.

  Version 1.0.0, February 7th, 2007

  Copyright (C) 2006-2007 University of Dublin, Trinity College, All Rights 
  Reserved

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the author(s) be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Author: Ladislav Kavan, ladislav.kavan@gmail.com

*/


#include <math.h>

// input: unit quaternion 'q0', translation vector 't' 
// output: unit dual quaternion 'dq'
void QuatTrans2UDQ(const float quaternion[4], const float translate[3], 
                  float dq[2][4])
{
   // non-dual part (just copy q0):
   for (int i=0; i<4; i++) dq[0][i] = quaternion[i];
   // dual part:
   dq[1][0] = -0.5*(translate[0]*quaternion[1] + translate[1]*quaternion[2] + translate[2]*quaternion[3]);
   dq[1][1] = 0.5*( translate[0]*quaternion[0] + translate[1]*quaternion[3] - translate[2]*quaternion[2]);
   dq[1][2] = 0.5*(-translate[0]*quaternion[3] + translate[1]*quaternion[0] + translate[2]*quaternion[1]);
   dq[1][3] = 0.5*( translate[0]*quaternion[2] - translate[1]*quaternion[1] + translate[2]*quaternion[0]);
}

// input: unit dual quaternion 'dq'
// output: unit quaternion 'q0', translation vector 't'
void UDQ2QuatTrans(const float dq[2][4], 
                  float q0[4], float t[3])
{
   // regular quaternion (just copy the non-dual part):
   for (int i=0; i<4; i++) q0[i] = dq[0][i];
   // translation vector:
   t[0] = 2.0*(-dq[1][0]*dq[0][1] + dq[1][1]*dq[0][0] - dq[1][2]*dq[0][3] + dq[1][3]*dq[0][2]);
   t[1] = 2.0*(-dq[1][0]*dq[0][2] + dq[1][1]*dq[0][3] + dq[1][2]*dq[0][0] - dq[1][3]*dq[0][1]);
   t[2] = 2.0*(-dq[1][0]*dq[0][3] - dq[1][1]*dq[0][2] + dq[1][2]*dq[0][1] + dq[1][3]*dq[0][0]);
}

// input: dual quat. 'dq' with non-zero non-dual part
// output: unit quaternion 'q0', translation vector 't'
void DQ2QuatTrans(const float dq[2][4], 
                  float q0[4], float t[3])
{
   float len = 0.0;
   for (int i=0; i<4; i++) len += dq[0][i] * dq[0][i];
   len = sqrt(len); 
   for (int i=0; i<4; i++) q0[i] = dq[0][i] / len;
   t[0] = 2.0*(-dq[1][0]*dq[0][1] + dq[1][1]*dq[0][0] - dq[1][2]*dq[0][3] + dq[1][3]*dq[0][2]) / len;
   t[1] = 2.0*(-dq[1][0]*dq[0][2] + dq[1][1]*dq[0][3] + dq[1][2]*dq[0][0] - dq[1][3]*dq[0][1]) / len;
   t[2] = 2.0*(-dq[1][0]*dq[0][3] - dq[1][1]*dq[0][2] + dq[1][2]*dq[0][1] + dq[1][3]*dq[0][0]) / len;
}	