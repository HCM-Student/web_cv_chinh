// Nạp danh sách "Cơ quan" vào tất cả <select data-coquan>
(function () {
  const JSON_URL = "/data/agencies.json";

  function populateSelect(select, groups) {
    // giữ lại option đầu (placeholder)
    const firstOpt = select.querySelector("option");
    select.innerHTML = "";
    if (firstOpt) select.appendChild(firstOpt);

    groups.forEach((g) => {
      const og = document.createElement("optgroup");
      og.label = g.name;
      (g.items || []).forEach((it) => {
        const opt = document.createElement("option");
        opt.value = it.id || it.name;
        opt.textContent = it.name;
        og.appendChild(opt);
      });
      select.appendChild(og);
    });
  }

  async function init() {
    const selects = document.querySelectorAll('select[data-coquan]');
    if (!selects.length) return;

    try {
      const res = await fetch(JSON_URL, { cache: "no-store" });
      const data = await res.json();
      selects.forEach((s) => populateSelect(s, data.groups || []));
    } catch (e) {
      console.error("Không tải được agencies.json:", e);
      // Fallback ngắn gọn để tránh vỡ UI
      const fallback = [
        { name: "Khối Trung ương", items: [{ id: "CP", name: "Chính phủ" }] },
        { name: "Bộ, ngành", items: [{ id: "BTNMT", name: "Bộ Tài nguyên và Môi trường" }] }
      ];
      selects.forEach((s) => populateSelect(s, fallback));
    }
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", init);
  } else {
    init();
  }
})();

