// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(function () {
	var THEME_KEY = "ciq-theme";
	var root = document.documentElement;

	function getInitialTheme() {
		var savedTheme = localStorage.getItem(THEME_KEY);
		if (savedTheme === "light" || savedTheme === "dark") {
			return savedTheme;
		}

		return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
	}

	function applyTheme(theme) {
		root.setAttribute("data-theme", theme);
		localStorage.setItem(THEME_KEY, theme);

		var button = document.getElementById("themeToggleBtn");
		var icon = document.getElementById("themeToggleIcon");
		var text = document.getElementById("themeToggleText");
		if (!button || !icon || !text) {
			return;
		}

		var nextThemeLabel = theme === "dark" ? "Light" : "Dark";
		icon.textContent = theme === "dark" ? "☀" : "🌙";
		text.textContent = nextThemeLabel;
		button.setAttribute("aria-label", "Switch to " + nextThemeLabel.toLowerCase() + " mode");
		button.setAttribute("title", "Switch to " + nextThemeLabel.toLowerCase() + " mode");
	}

	function bindToggle() {
		var button = document.getElementById("themeToggleBtn");
		if (!button) {
			return;
		}

		applyTheme(root.getAttribute("data-theme") || "light");

		button.addEventListener("click", function () {
			var activeTheme = root.getAttribute("data-theme") || "light";
			applyTheme(activeTheme === "dark" ? "light" : "dark");
		});
	}

	applyTheme(getInitialTheme());
	document.addEventListener("DOMContentLoaded", bindToggle);
})();
