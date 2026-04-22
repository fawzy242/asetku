import Swal from "sweetalert2";
import ReactDOM from "react-dom/client";

class ConfirmDialog {
  // Helper untuk cleanup React root jika ada
  static cleanupReactRoots() {
    try {
      const roots = document.querySelectorAll('[data-react-root]');
      roots.forEach(root => {
        const reactRoot = root._reactRoot;
        if (reactRoot) {
          reactRoot.unmount();
        }
      });
    } catch (e) {
      // Ignore
    }
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

    // Cleanup sebelum show
    this.cleanupReactRoots();
    if (Swal.isVisible()) {
      Swal.close();
    }

    try {
      const result = await Swal.fire({
        title,
        text,
        icon,
        showCancelButton,
        confirmButtonColor,
        cancelButtonColor,
        confirmButtonText,
        cancelButtonText,
        background: isDark ? "#1f2937" : "#ffffff",
        color: isDark ? "#f9fafb" : "#111827",
        iconColor: icon === "warning" ? "#f59e0b" : icon === "error" ? "#ef4444" : icon === "success" ? "#10b981" : "#3b82f6",
        backdrop: true,
        allowOutsideClick: true,
        allowEscapeKey: true,
        allowEnterKey: true,
        returnFocus: true,
        focusConfirm: true,
        focusCancel: false,
        customClass: {
          popup: "swal-popup",
          title: "swal-title",
          htmlContainer: "swal-text",
          confirmButton: "swal-confirm-btn",
          cancelButton: "swal-cancel-btn",
        },
        didOpen: () => {
          const popup = Swal.getPopup();
          if (popup) {
            popup.style.borderRadius = "16px";
            popup.style.padding = "24px";
          }
        },
        didClose: () => {
          this.cleanupReactRoots();
        },
        willClose: () => {
          this.cleanupReactRoots();
        },
      });

      // Pastikan cleanup setelah result
      this.cleanupReactRoots();
      
      return result.isConfirmed;
    } catch (error) {
      console.error("Swal error:", error);
      this.cleanupReactRoots();
      Swal.close();
      return false;
    }
  }

  static async showDelete(title = "Are you sure?", text = "This item will be permanently deleted.") {
    return this.show({
      title,
      text,
      icon: "warning",
      confirmButtonText: "Delete",
      cancelButtonText: "Cancel",
      confirmButtonColor: "#dc2626",
    });
  }

  static async showSuccess(title = "Success!", text = "") {
    const isDark = document.documentElement.getAttribute("data-theme") === "dark";

    this.cleanupReactRoots();
    if (Swal.isVisible()) {
      Swal.close();
    }

    try {
      const result = await Swal.fire({
        title,
        text,
        icon: "success",
        confirmButtonColor: "#dc2626",
        background: isDark ? "#1f2937" : "#ffffff",
        color: isDark ? "#f9fafb" : "#111827",
        backdrop: true,
        allowOutsideClick: true,
        allowEscapeKey: true,
        allowEnterKey: true,
        returnFocus: true,
        focusConfirm: true,
        customClass: {
          popup: "swal-popup",
          confirmButton: "swal-confirm-btn",
        },
        didClose: () => {
          this.cleanupReactRoots();
        },
        willClose: () => {
          this.cleanupReactRoots();
        },
      });

      this.cleanupReactRoots();
      return result;
    } catch (error) {
      console.error("Swal error:", error);
      this.cleanupReactRoots();
      Swal.close();
      return null;
    }
  }

  static async showError(title = "Error!", text = "") {
    const isDark = document.documentElement.getAttribute("data-theme") === "dark";

    this.cleanupReactRoots();
    if (Swal.isVisible()) {
      Swal.close();
    }

    try {
      const result = await Swal.fire({
        title,
        text,
        icon: "error",
        confirmButtonColor: "#dc2626",
        background: isDark ? "#1f2937" : "#ffffff",
        color: isDark ? "#f9fafb" : "#111827",
        backdrop: true,
        allowOutsideClick: true,
        allowEscapeKey: true,
        allowEnterKey: true,
        returnFocus: true,
        focusConfirm: true,
        customClass: {
          popup: "swal-popup",
          confirmButton: "swal-confirm-btn",
        },
        didClose: () => {
          this.cleanupReactRoots();
        },
        willClose: () => {
          this.cleanupReactRoots();
        },
      });

      this.cleanupReactRoots();
      return result;
    } catch (error) {
      console.error("Swal error:", error);
      this.cleanupReactRoots();
      Swal.close();
      return null;
    }
  }

  static async showInfo(title = "Info", text = "") {
    const isDark = document.documentElement.getAttribute("data-theme") === "dark";

    this.cleanupReactRoots();
    if (Swal.isVisible()) {
      Swal.close();
    }

    try {
      const result = await Swal.fire({
        title,
        text,
        icon: "info",
        confirmButtonColor: "#dc2626",
        background: isDark ? "#1f2937" : "#ffffff",
        color: isDark ? "#f9fafb" : "#111827",
        backdrop: true,
        allowOutsideClick: true,
        allowEscapeKey: true,
        allowEnterKey: true,
        returnFocus: true,
        focusConfirm: true,
        customClass: {
          popup: "swal-popup",
          confirmButton: "swal-confirm-btn",
        },
        didClose: () => {
          this.cleanupReactRoots();
        },
        willClose: () => {
          this.cleanupReactRoots();
        },
      });

      this.cleanupReactRoots();
      return result;
    } catch (error) {
      console.error("Swal error:", error);
      this.cleanupReactRoots();
      Swal.close();
      return null;
    }
  }

  static async showWarning(title = "Warning!", text = "") {
    const isDark = document.documentElement.getAttribute("data-theme") === "dark";

    this.cleanupReactRoots();
    if (Swal.isVisible()) {
      Swal.close();
    }

    try {
      const result = await Swal.fire({
        title,
        text,
        icon: "warning",
        confirmButtonColor: "#dc2626",
        background: isDark ? "#1f2937" : "#ffffff",
        color: isDark ? "#f9fafb" : "#111827",
        backdrop: true,
        allowOutsideClick: true,
        allowEscapeKey: true,
        allowEnterKey: true,
        returnFocus: true,
        focusConfirm: true,
        customClass: {
          popup: "swal-popup",
          confirmButton: "swal-confirm-btn",
        },
        didClose: () => {
          this.cleanupReactRoots();
        },
        willClose: () => {
          this.cleanupReactRoots();
        },
      });

      this.cleanupReactRoots();
      return result;
    } catch (error) {
      console.error("Swal error:", error);
      this.cleanupReactRoots();
      Swal.close();
      return null;
    }
  }

  static close() {
    this.cleanupReactRoots();
    Swal.close();
  }
}

export default ConfirmDialog;