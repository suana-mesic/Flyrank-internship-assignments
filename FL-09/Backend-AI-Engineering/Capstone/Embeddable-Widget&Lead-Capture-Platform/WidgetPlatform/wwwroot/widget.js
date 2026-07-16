(function () {
  var me = document.currentScript;
  var widgetId = me.getAttribute("data-widget-id");
  var base = new URL(me.src).origin;

  var box = document.createElement("div");
  me.parentNode.insertBefore(box, me);

  fetch(base + "/widgets/" + widgetId + "/config")
    .then(function (r) {
      if (!r.ok) throw new Error("config " + r.status);
      return r.json();
    })
    .then(function (cfg) {
      render(cfg);
    })
    .catch(function (e) {
      console.error("[widget]", e);
    });

  function render(cfg) {
    if (cfg.type === "cta") {
      box.innerHTML = '<a href="#">' + escape(cfg.title) + "</a>";
      return;
    }

    var inputs = cfg.fields
      .map(function (f) {
        return (
          "<label>" +
          escape(f.label) +
          "<br>" +
          '<input name="' +
          escape(f.name) +
          '"></label><br>'
        );
      })
      .join("");

    box.innerHTML =
      '<div style="border:1px solid #ccc;padding:16px;max-width:320px;font-family:sans-serif">' +
      "<h3>" +
      escape(cfg.title) +
      "</h3>" +
      inputs +
      '<input name="website" style="display:none" tabindex="-1" autocomplete="off">' +
      '<button type="button">Pošalji</button>' +
      '<p class="msg"></p>' +
      "</div>";

    box.querySelector("button").addEventListener("click", function () {
      send(cfg);
    });
  }

  function send(cfg) {
    var data = {};
    cfg.fields.forEach(function (f) {
      data[f.name] = box.querySelector('[name="' + f.name + '"]').value;
    });

    fetch(base + "/submissions", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        widgetId: widgetId,
        data: data,
        website: box.querySelector('[name="website"]').value,
      }),
    })
      .then(function (r) {
        box.querySelector(".msg").textContent = r.ok
          ? "Hvala!"
          : "Greška (" + r.status + ")";
      })
      .catch(function () {
        box.querySelector(".msg").textContent = "Greška u mreži";
      });
  }

  function escape(s) {
    var d = document.createElement("div");
    d.textContent = s;
    return d.innerHTML;
  }
})();
