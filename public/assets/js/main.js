/**
 * ProManager Online — site interactions.
 *
 * Kept intentionally tiny and dependency-free so the site loads instantly and
 * works under a strict Content-Security-Policy (no inline scripts).
 *
 * Responsibilities:
 *   1. Toggle the mobile navigation menu and keep ARIA state in sync.
 *   2. Mark the current page's nav link with aria-current for styling + a11y.
 *   3. Close the mobile menu when a link is tapped or focus leaves the header.
 */
(function () {
  "use strict";

  /** Wire up the mobile navigation toggle button. */
  function initNavToggle() {
    var toggle = document.querySelector(".nav-toggle");
    var nav = document.getElementById("site-nav");
    if (!toggle || !nav) {
      return;
    }

    toggle.addEventListener("click", function () {
      var isOpen = nav.classList.toggle("open");
      toggle.setAttribute("aria-expanded", String(isOpen));
    });

    // Collapse the menu after a navigation link is chosen (mobile).
    nav.addEventListener("click", function (event) {
      if (event.target.closest("a")) {
        nav.classList.remove("open");
        toggle.setAttribute("aria-expanded", "false");
      }
    });

    // Collapse the menu on Escape for keyboard users.
    document.addEventListener("keydown", function (event) {
      if (event.key === "Escape" && nav.classList.contains("open")) {
        nav.classList.remove("open");
        toggle.setAttribute("aria-expanded", "false");
        toggle.focus();
      }
    });
  }

  /**
   * Highlight the navigation entry that matches the current document, so the
   * active page is obvious and announced to assistive technology.
   */
  function markCurrentPage() {
    var path = window.location.pathname.split("/").pop() || "index.html";
    var links = document.querySelectorAll(".site-nav a[href]");
    links.forEach(function (link) {
      var href = link.getAttribute("href");
      if (href === path || (path === "index.html" && href === "index.html")) {
        link.setAttribute("aria-current", "page");
      }
    });
  }

  document.addEventListener("DOMContentLoaded", function () {
    initNavToggle();
    markCurrentPage();
  });
})();
