'use server'

import { httpPut } from "@/_lib/server/query-api";
import { clearMustChangePassword } from "@/_lib/server/session";
import { redirect } from "next/navigation";

export async function changePasswordOnLogin(
  prevState: { error: string },
  formData: FormData
): Promise<{ error: string }> {

  const currentPassword = formData.get('currentPassword');
  const newPassword = formData.get('newPassword');
  const confirmPassword = formData.get('confirmPassword');

  // Validate passwords match
  if (newPassword !== confirmPassword) {
    return { error: "New passwords do not match" };
  }

  // Validate password is not empty
  if (!newPassword || newPassword.toString().trim() === '') {
    return { error: "Password cannot be empty" };
  }

  try {
    const body = {
      currentPassword,
      newPassword,
      confirmPassword
    };

    const response = await httpPut({ url: 'profile/changepassword', body });

    if (!response.ok) {
      const responseText = await response.text();
      console.log(responseText);
      return { error: "Failed to change password" };
    }

    await response.text();

    // Clear the mustChangePassword flag
    await clearMustChangePassword();

    // Redirect to home
    redirect('/home/work');
  } catch (error) {
    console.error('Error changing password:', error);
    return { error: "An error occurred while changing password" };
  }
}
