function showLoginForm() {
  document.getElementById("login-form").classList.remove("hidden");
  document.getElementById("register-form").classList.add("hidden");
  document.getElementById("auth-tab-login").classList.add("active");
  document.getElementById("auth-tab-register").classList.remove("active");
  document.getElementById("auth-tab-login").setAttribute("aria-pressed", "true");
  document.getElementById("auth-tab-register").setAttribute("aria-pressed", "false");
  document.querySelector(".auth-heading").textContent = "Welcome back";
  document.querySelector(".auth-subheading").textContent = "Sign in to continue to your account.";
}

function showRegisterForm() {
  document.getElementById("register-form").classList.remove("hidden");
  document.getElementById("login-form").classList.add("hidden");
  document.getElementById("auth-tab-register").classList.add("active");
  document.getElementById("auth-tab-login").classList.remove("active");
  document.getElementById("auth-tab-register").setAttribute("aria-pressed", "true");
  document.getElementById("auth-tab-login").setAttribute("aria-pressed", "false");
  document.querySelector(".auth-heading").textContent = "Join BeeMart";
  document.querySelector(".auth-subheading").textContent = "Create an account to get started.";
}
