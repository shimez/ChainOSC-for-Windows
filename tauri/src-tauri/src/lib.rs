use std::{net::{SocketAddr, ToSocketAddrs, UdpSocket}, process::Command};
use tauri::{
    menu::{Menu, MenuItem},
    tray::{TrayIconBuilder, TrayIconEvent},
    Manager, WindowEvent,
};

const AUTOSTART_NAME: &str = "ChainOSCForWindows";
const AUTOSTART_KEY: &str = r"HKCU\Software\Microsoft\Windows\CurrentVersion\Run";

#[cfg(windows)]
struct InstanceGuard(windows_sys::Win32::Foundation::HANDLE);

#[cfg(windows)]
impl Drop for InstanceGuard {
    fn drop(&mut self) {
        unsafe { windows_sys::Win32::Foundation::CloseHandle(self.0) };
    }
}

#[cfg(not(windows))]
struct InstanceGuard;

#[cfg(windows)]
fn single_instance_or_activate() -> Option<InstanceGuard> {
    use std::{iter::once, ptr::null};
    use windows_sys::Win32::{
        Foundation::{GetLastError, ERROR_ALREADY_EXISTS},
        System::Threading::CreateMutexW,
        UI::WindowsAndMessaging::{FindWindowW, SetForegroundWindow, ShowWindow, SW_RESTORE},
    };
    let mutex_name: Vec<u16> = "Local\\ChainOSCForWindows.SingleInstance".encode_utf16().chain(once(0)).collect();
    let handle = unsafe { CreateMutexW(null(), 1, mutex_name.as_ptr()) };
    if handle.is_null() {
        return Some(InstanceGuard(handle));
    }
    if unsafe { GetLastError() } == ERROR_ALREADY_EXISTS {
        let title: Vec<u16> = "ChainOSC for Windows".encode_utf16().chain(once(0)).collect();
        let window = unsafe { FindWindowW(null(), title.as_ptr()) };
        if !window.is_null() {
            unsafe {
                ShowWindow(window, SW_RESTORE);
                SetForegroundWindow(window);
            }
        }
        unsafe { windows_sys::Win32::Foundation::CloseHandle(handle) };
        return None;
    }
    Some(InstanceGuard(handle))
}

#[cfg(not(windows))]
fn single_instance_or_activate() -> Option<InstanceGuard> {
    Some(InstanceGuard)
}

#[tauri::command]
fn get_autostart() -> Result<bool, String> {
    let output = Command::new("reg")
        .args(["query", AUTOSTART_KEY, "/v", AUTOSTART_NAME])
        .output()
        .map_err(|error| format!("Autostart status could not be read: {error}"))?;
    Ok(output.status.success())
}

#[tauri::command]
fn set_autostart(enabled: bool) -> Result<(), String> {
    if enabled && cfg!(debug_assertions) {
        return Err("Start with Windows can only be enabled from a release build.".into());
    }
    let status = if enabled {
        let executable = std::env::current_exe().map_err(|error| error.to_string())?;
        let command = format!("\"{}\" --autostart", executable.display());
        Command::new("reg").args(["add", AUTOSTART_KEY, "/v", AUTOSTART_NAME, "/t", "REG_SZ", "/d", &command, "/f"]).status()
    } else {
        Command::new("reg").args(["delete", AUTOSTART_KEY, "/v", AUTOSTART_NAME, "/f"]).status()
    }.map_err(|error| format!("Autostart setting could not be changed: {error}"))?;
    if enabled && !status.success() {
        return Err("Windows rejected the autostart setting.".into());
    }
    // Deleting a value that is already absent is equivalent to disabled.
    Ok(())
}

fn show_main_window(app: &tauri::AppHandle) {
    if let Some(window) = app.get_webview_window("main") {
        let _ = window.unminimize();
        let _ = window.show();
        let _ = window.set_focus();
    }
}

fn push_osc_string(packet: &mut Vec<u8>, value: &str) -> Result<(), String> {
    if value.as_bytes().contains(&0) {
        return Err("OSC strings cannot contain a null character.".into());
    }
    packet.extend_from_slice(value.as_bytes());
    packet.push(0);
    while packet.len() % 4 != 0 {
        packet.push(0);
    }
    Ok(())
}

fn build_osc_packet(address: &str, value_type: &str, value: &str) -> Result<Vec<u8>, String> {
    if !address.starts_with('/') {
        return Err("OSC Address must start with '/'.".into());
    }
    let mut packet = Vec::with_capacity(128);
    push_osc_string(&mut packet, address)?;
    match value_type {
        "int" => {
            push_osc_string(&mut packet, ",i")?;
            let parsed = value
                .parse::<i32>()
                .map_err(|_| "Value is not a valid 32-bit integer.".to_string())?;
            packet.extend_from_slice(&parsed.to_be_bytes());
        }
        "float" => {
            push_osc_string(&mut packet, ",f")?;
            let parsed = value
                .parse::<f32>()
                .map_err(|_| "Value is not a valid 32-bit float.".to_string())?;
            if !parsed.is_finite() {
                return Err("Float value must be finite.".into());
            }
            packet.extend_from_slice(&parsed.to_bits().to_be_bytes());
        }
        "string" => {
            push_osc_string(&mut packet, ",s")?;
            push_osc_string(&mut packet, value)?;
        }
        _ => return Err("OSC value type must be int, float, or string.".into()),
    }
    Ok(packet)
}

#[tauri::command]
fn send_osc(
    host: String,
    port: u16,
    address: String,
    value_type: String,
    value: String,
) -> Result<usize, String> {
    let destination = (host.as_str(), port)
        .to_socket_addrs()
        .map_err(|error| format!("OSC destination could not be resolved: {error}"))?
        .next()
        .ok_or_else(|| "OSC destination could not be resolved.".to_string())?;
    let bind_address = match destination {
        SocketAddr::V4(_) => "0.0.0.0:0",
        SocketAddr::V6(_) => "[::]:0",
    };
    let socket = UdpSocket::bind(bind_address)
        .map_err(|error| format!("UDP socket could not be opened: {error}"))?;
    let packet = build_osc_packet(&address, &value_type, &value)?;
    socket
        .send_to(&packet, destination)
        .map_err(|error| format!("OSC message could not be sent: {error}"))
}

#[tauri::command]
fn write_text_file(path: String, content: String) -> Result<(), String> {
    std::fs::write(path, content).map_err(|error| format!("The file could not be written: {error}"))
}

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    let Some(_instance_guard) = single_instance_or_activate() else {
        return;
    };
    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .plugin(tauri_plugin_dialog::init())
        .plugin(tauri_plugin_global_shortcut::Builder::new().build())
        .setup(|app| {
            let show = MenuItem::with_id(app, "show", "Show ChainOSC", true, None::<&str>)?;
            let quit = MenuItem::with_id(app, "quit", "Exit", true, None::<&str>)?;
            let menu = Menu::with_items(app, &[&show, &quit])?;
            let mut tray = TrayIconBuilder::new()
                .menu(&menu)
                .tooltip("ChainOSC for Windows")
                .show_menu_on_left_click(false)
                .on_menu_event(|app, event| match event.id().as_ref() {
                    "show" => show_main_window(app),
                    "quit" => app.exit(0),
                    _ => {}
                });
            if let Some(icon) = app.default_window_icon() {
                tray = tray.icon(icon.clone());
            }
            tray.build(app)?;
            if std::env::args().any(|argument| argument == "--autostart") {
                if let Some(window) = app.get_webview_window("main") {
                    let _ = window.hide();
                }
            }
            Ok(())
        })
        .on_tray_icon_event(|app, event| {
            if matches!(event, TrayIconEvent::DoubleClick { .. }) {
                show_main_window(app);
            }
        })
        .on_window_event(|window, event| match event {
            WindowEvent::CloseRequested { api, .. } => {
                api.prevent_close();
                let _ = window.hide();
            }
            WindowEvent::Resized(_) if window.is_minimized().unwrap_or(false) => {
                let _ = window.hide();
            }
            _ => {}
        })
        .invoke_handler(tauri::generate_handler![send_osc, get_autostart, set_autostart, write_text_file])
        .run(tauri::generate_context!())
        .expect("error while running ChainOSC for Windows");
}

#[cfg(test)]
mod tests {
    use super::build_osc_packet;

    #[test]
    fn builds_big_endian_integer_packet() {
        assert_eq!(
            build_osc_packet("/test", "int", "1").unwrap(),
            vec![0x2f, 0x74, 0x65, 0x73, 0x74, 0, 0, 0, 0x2c, 0x69, 0, 0, 0, 0, 0, 1,]
        );
    }

    #[test]
    fn accepts_osc_int32_boundaries() {
        assert!(build_osc_packet("/test", "int", "-2147483648").is_ok());
        assert!(build_osc_packet("/test", "int", "2147483647").is_ok());
    }

    #[test]
    fn rejects_values_outside_osc_int32() {
        assert!(build_osc_packet("/test", "int", "-2147483649").is_err());
        assert!(build_osc_packet("/test", "int", "2147483648").is_err());
    }

    #[test]
    fn accepts_finite_float32_and_rejects_non_finite_values() {
        assert!(build_osc_packet("/test", "float", "3.4028234e38").is_ok());
        assert!(build_osc_packet("/test", "float", "3.5e38").is_err());
        assert!(build_osc_packet("/test", "float", "NaN").is_err());
        assert!(build_osc_packet("/test", "float", "Infinity").is_err());
    }
}
