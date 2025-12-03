// Utility function to check if response requires login redirect
const followIfLoginRedirect = (response: Response): void => {
  // If the response indicates a redirect to login page, follow it
  if (response.status === 401) {
    window.location.href = '/Identity/Account/Login';
  }
};

export default followIfLoginRedirect;
