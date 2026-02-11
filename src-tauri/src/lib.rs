mod balanced_rand;
use balanced_rand::BalancedRand;
use balanced_rand::BalancedRandPlane;

#[tauri::command]
fn draw_id(min_id: u32, max_id: u32) -> Result<String, String> {
    if min_id >= max_id {
        return Err(format!("最小ID({})必须小于最大ID({})", min_id, max_id));
    }
    
    if max_id - min_id + 1 < 3 {
        return Err("ID范围太小，至少需要3个不同的ID".to_string());
    }
    
    let result = std::panic::catch_unwind(|| {
        BalancedRand::new_from_range(
            min_id, max_id,
            3,        // 最小候选池大小
            5,        // 最大差距阈值
            2.0,      // 冷启动提升系数
            0.7,      // 衰减因子
            true,     // 加载历史数据
        )
        .and_then(|mut br| br.draw(true))
    });
    
    match result {
        Ok(Ok(id)) => Ok(id.to_string()),
        Ok(Err(e)) => Err(format!("抽取失败: {}", e)),
        Err(_) => Err("内部错误: 程序发生panic".to_string()),
    }
}

#[tauri::command]
fn draw_plane(row_num: u32, col_num: u32) -> Result<String, String> {
    let result = std::panic::catch_unwind(|| {
        BalancedRandPlane::new(
            row_num, col_num,
            3,        // 最小候选池大小
            5,        // 最大差距阈值
            2.0,      // 冷启动提升系数
            0.7,      // 衰减因子
            true,     // 加载历史数据
        )
        .and_then(|mut brp| brp.draw_position(true))
    });
    
    match result {
        Ok(Ok(pos)) => Ok(format!("{}行{}列", pos.0, pos.1)),
        Ok(Err(e)) => Err(format!("抽取失败: {}", e)),
        Err(_) => Err("内部错误: 程序发生panic".to_string()),
    }
}

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .invoke_handler(tauri::generate_handler![draw_id,draw_plane])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
