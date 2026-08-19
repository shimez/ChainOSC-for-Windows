use std::net::{SocketAddr, ToSocketAddrs, UdpSocket};

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

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .plugin(tauri_plugin_global_shortcut::Builder::new().build())
        .invoke_handler(tauri::generate_handler![send_osc])
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
}
