import { DirectiveOptions } from "vue";
/**
 * 1. 文本字段
 * edit 指令-是否编辑
 * v-edit:[status]
 * status： 增/删/改 状态
 * 2. 集合字段
 * v-edit:[status]="Array<any>"
 * list 过滤集合
 * key 集合中v-model对应字段名
 * label 集合中 需要展示的字段名
 */
const domStyleFn = (el, { value, arg }, vnode) => {
  // const { value, arg } = binding; // value Array<any>
  const modelVal = vnode.data.model.value; // v-model 值
  let htmlVal: string = modelVal; // 内容value
  let elPre = el.previousSibling || {}; // el前dom，insert添加的dom
  if (Array.isArray(value) && modelVal) {
    try {
      // 参数判断
      htmlVal = value.filter(res => res["Value"] === modelVal)[0]["Text"];
    } catch (error) {
      console.warn("dirEdit,指令结构不符合要求", error);
      htmlVal = modelVal;
    }
  }
  // 添加&修改 展示组件
  if (arg === "add" || arg === "edit") {
    el.style.display = "inline-block";
    elPre.style.display = "none";
  } else {
    el.style.display = "none";
    elPre.innerHTML = htmlVal;
    elPre.style.display = "inline-block";
  }
};
// 编辑
const edit: DirectiveOptions = {
  inserted: (el, { value, arg, expression }, vnode) => {
    // el前添加dom
    const div = document.createElement("div");
    div.innerHTML = `<span id="${expression}" style="display: none;"></span>`;
    el.parentNode && el.parentNode.insertBefore(div.childNodes[0], el);
    domStyleFn(el, { value, arg }, vnode);
  },
  update: (el, { value, arg }, vnode) => domStyleFn(el, { value, arg }, vnode)
};

export default edit;
