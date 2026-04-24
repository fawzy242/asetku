import Swal from "sweetalert2";

class ConfirmDialog {
  static #cleanup() {
    document.body.style.overflow = "";
    document.body.style.paddingRight = "";
  }

  static async show({
    title = "Are you sure?",
    text = "",
    icon = "warning",
    confirmButtonText = "Yes",
    cancelButtonText = "Cancel",
    confirmButtonColor = "#dc2626",
    cancelButtonColor = "#6b7280",
    showCancelButton = true,
  }) {
    const isDark = document.documentElement.getAttribute("data-theme") === "dark";
    if (Swal.isVisible()) Swal.close();

    try {
      const result = await Swal.fire({
        title, text, icon, showCancelButton, confirmButtonColor, cancelButtonColor, confirmButtonText, cancelButtonText,
        background: isDark ? "#1f2937" : "#ffffff",
        color: isDark ? "#f9fafb" : "#111827",
        iconColor: icon === "warning" ? "#f59e0b" : icon === "error" ? "#ef4444" : icon === "success" ? "#10b981" : "#3b82f6",
        allowOutsideClick: true, allowEscapeKey: true, allowEnterKey: true,
        customClass: {
          popup: "swal-popup", title: "swal-title", htmlContainer: "swal-text",
          confirmButton: "swal-confirm-btn", cancelButton: "swal-cancel-btn",
        },
        didClose: () => this.#cleanup(),
        willClose: () => this.#cleanup(),
      });
      this.#cleanup();
      return result.isConfirmed;
    } catch {
      this.#cleanup();
      Swal.close();
      return false;
    }
  }

  static async showDelete(title = "Are you sure?", text = "This item will be permanently deleted.") {
    return this.show({ title, text, icon: "warning", confirmButtonText: "Delete", cancelButtonText: "Cancel", confirmButtonColor: "#dc2626" });
  }

  static async showSuccess(title = "Success!", text = "") {
    const isDark = document.documentElement.getAttribute("data-theme") === "dark";
    if (Swal.isVisible()) Swal.close();
    try {
      const result = await Swal.fire({
        title, text, icon: "success", confirmButtonColor: "#dc2626",
        background: isDark ? "#1f2937" : "#ffffff", color: isDark ? "#f9fafb" : "#111827",
        allowOutsideClick: true, allowEscapeKey: true, allowEnterKey: true,
        customClass: { popup: "swal-popup", confirmButton: "swal-confirm-btn" },
        didClose: () => this.#cleanup(), willClose: () => this.#cleanup(),
      });
      this.#cleanup();
      return result;
    } catch { this.#cleanup(); Swal.close(); return null; }
  }

  static async showError(title = "Error!", text = "") {
    const isDark = document.documentElement.getAttribute("data-theme") === "dark";
    if (Swal.isVisible()) Swal.close();
    try {
      const result = await Swal.fire({
        title, text, icon: "error", confirmButtonColor: "#dc2626",
        background: isDark ? "#1f2937" : "#ffffff", color: isDark ? "#f9fafb" : "#111827",
        allowOutsideClick: true, allowEscapeKey: true, allowEnterKey: true,
        customClass: { popup: "swal-popup", confirmButton: "swal-confirm-btn" },
        didClose: () => this.#cleanup(), willClose: () => this.#cleanup(),
      });
      this.#cleanup();
      return result;
    } catch { this.#cleanup(); Swal.close(); return null; }
  }

  static close() { this.#cleanup(); Swal.close(); }
}

export default ConfirmDialog;