// ==============================================================================
var gui = {
   
   // Slider
   sigma  : {value: 0.1, min: 0.01, max: 0.5, step: 0.01, text: "Rugosité (sigma)"    },
   refract: {value: 1.5, min: 1.01, max: 3,   step: 0.01, text: "Indice de réfraction"},

   // Checkbox
   lockLight: {value: true, text: "Lock light"},
   RefractOrReflect: {value: true, text: "Réflection"},

   // Color Selector
   color: {value: "#aaaaaa", mesh: null, text: "Couleur du matériau"},

   // RadioGroup
   beckmann: {id: 0, value: 0,  text: "Beckmann"},
   ggx:      {id: 1, value: 1, text: "GGX"      },

   // Select
   obj_select: {
      value: "bunny", 
      values: [
         "bunny", 
         "mustang", 
         "porsche", 
         "sphere",
         "evoli",
         "hericendre"
      ], 
      mesh: null, 
      text: "Choisissez un objet"
   },
}



// ==============================================================================
function initGui() {

   let body = document.getElementsByClassName('gui')[0];

   let table = document.createElement('table');
   body.appendChild(table);

   gui.guiBody = document.createElement('tbody');
   table.appendChild(gui.guiBody);

   // Shader section
   sec = gui_section("Options du shader");
   gui_slider(sec, gui.sigma);
   gui_slider(sec, gui.refract);
   gui_vspace(sec);
   gui_text(sec, "Facteur D (shader)");
   gui_radiobutton(sec, gui.beckmann, "dShader", true);
   gui_radiobutton(sec, gui.ggx,      "dShader"      );
   gui_vspace(sec);

   // Object section
   sec = gui_section("Options de l'objet");
   gui_colorSelector(sec, gui.color);
   gui_vspace(sec);
   gui_select(sec, gui.obj_select);

   // Ohter options
   sec = gui_section("Autres options");
   gui_checkbox(sec, gui.lockLight);
   gui_vspace(sec);
}


// ==============================================================================
function gui_section(title) {

   let tr = document.createElement('tr');
   gui.guiBody.appendChild(tr);

   let td = document.createElement('td');
   td.appendChild(document.createTextNode(title));
   tr.appendChild(td);

   tr = document.createElement('tr');
   gui.guiBody.appendChild(tr);
   td = document.createElement('td');
   tr.appendChild(td);

   return td;
}


// ==============================================================================
function gui_vspace(sec) {

   let par = document.createElement('p');
   par.className = 'p';
   sec.appendChild(par);
}


// ==============================================================================
function gui_text(sec, text) {

   sec.appendChild(document.createTextNode(text));
   sec.appendChild(document.createElement('br'));
}


// ==============================================================================
function gui_info(sec, obj) {

   obj.id = document.createTextNode('');
   gui_info_update(obj);
   sec.appendChild(obj.id);
   sec.appendChild(document.createElement('br'));
}


// ==============================================================================
function gui_info_update(obj) {

   obj.id.nodeValue = obj.text + ': ' + obj.value + ' ' + obj.unit;
}


// ==============================================================================
function gui_checkbox(sec, obj) {

   let box = document.createElement('input');
   box.type      = 'checkbox';
   box.className = 'checkbox';
   box.checked   = obj.value;
   obj.checkbox  = box;

   box.addEventListener(
      'change',
      function() {
         obj.value = box.checked;
      }
   );

   let label = document.createElement('label');
   label.innerHTML = obj.text;
   label.className = 'label';

   sec.appendChild(box);
   sec.appendChild(label);
   sec.appendChild(document.createElement('br'));
}


// ==============================================================================
function gui_slider(sec, obj) {

   let slider = document.createElement('input');
   slider.type      = 'range';
   slider.className = 'slider';
   slider.min       = obj.min;
   slider.max       = obj.max;
   slider.step      = obj.step;
   slider.id        = obj.text;
   slider.value     = obj.value;


   obj.slider = slider;
   txt = document.createTextNode(obj.text + ': ' + slider.value);
   obj.slider.txt = txt;

   obj.slider.addEventListener(
      'input',
      function() {
         obj.value = obj.slider.value;
            obj.slider.txt.nodeValue = slider.id + ': ' + slider.value;
      }
   );

   sec.appendChild(txt);
   sec.appendChild(slider);
   sec.appendChild(document.createElement('br'));
}


// ==============================================================================
function gui_div(sec, id = null) {

   let div = document.createElement("div");
   div.id = id;
   sec.appendChild(div);

   return div;
}


// ==============================================================================
function gui_radiobutton(sec, obj, name, checked = false) {

   let rad = document.createElement("input");
   rad.type    = "radio";
   rad.checked = checked;
   rad.id      = name + "_" + obj.id;
   rad.value   = obj.value;
   rad.name    = name;

   rad.addEventListener(
      'input',
      function(){
         window.localStorage.setItem(name, JSON.stringify(obj.id));
      }
   );
   
   if (checked) window.localStorage.setItem(name, JSON.stringify(obj.id));

   let label = document.createElement("label");
   label.innerText = obj.text;

   label.appendChild(rad);
   sec.appendChild(label);
}


// ==============================================================================
function gui_button(sec, obj) {
   
   let button = document.createElement("button");
   button.innerHTML = obj.text;
   button.addEventListener(
      'click',
      obj.func
   )

   sec.appendChild(button);
   sec.appendChild(document.createElement('br'));
}


// ==============================================================================
function gui_colorSelector(sec, obj) {

   let colSel = document.createElement("input");
   colSel.type = "color";
   colSel.className = "colorSelector";
   colSel.value = obj.value;

   colSel.addEventListener(
      'change',
      function() {
         obj.value = colSel.value;

         if(obj.mesh){
            obj.mesh.setColor(obj.value);
         }
      }
   );

   txt = document.createTextNode(obj.text);

   sec.appendChild(txt);
   sec.appendChild(colSel);
   sec.appendChild(document.createElement('br'));
}


// ==============================================================================
function gui_select(sec, obj) {

   let select = document.createElement("select");
   select.multiple = true;

   for(let i=0; i < obj.values.length; i++){
      let val = obj.values[i];
      let opt = document.createElement("option");
      opt.value = val;
      opt.text = val;
      if(i == 0)
         opt.selected = true;

      select.appendChild(opt);
   }

   select.addEventListener(
      'change',
      function() {
         obj.value = select.value;
         setOBJ(obj.value);
      }
   );

   txt = document.createTextNode(obj.text);

   sec.appendChild(txt);
   sec.appendChild(select);
   sec.appendChild(document.createElement('br'));
}


// END gui.js
