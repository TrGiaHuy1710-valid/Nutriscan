from langgraph.graph import END, START, StateGraph

from app.modules.food.graph.nodes import (
    analyze_image_node,
    ask_user_node,
    check_missing_info_node,
    extract_user_info_node,
    finalize_result_node,
)
from app.modules.food.graph.state import FoodSessionState


def route_by_status(state: FoodSessionState) -> str:
    if state.get("status") == "ready":
        return "finalize_result"
    return "ask_user"


def build_image_graph():
    graph = StateGraph(FoodSessionState)

    graph.add_node("analyze_image", analyze_image_node)
    graph.add_node("check_missing_info", check_missing_info_node)
    graph.add_node("ask_user", ask_user_node)
    graph.add_node("finalize_result", finalize_result_node)

    graph.add_edge(START, "analyze_image")
    graph.add_edge("analyze_image", "check_missing_info")
    graph.add_conditional_edges(
        "check_missing_info",
        route_by_status,
        {
            "ask_user": "ask_user",
            "finalize_result": "finalize_result",
        },
    )
    graph.add_edge("ask_user", END)
    graph.add_edge("finalize_result", END)

    return graph.compile()


def build_chat_graph():
    graph = StateGraph(FoodSessionState)

    graph.add_node("extract_user_info", extract_user_info_node)
    graph.add_node("check_missing_info", check_missing_info_node)
    graph.add_node("ask_user", ask_user_node)
    graph.add_node("finalize_result", finalize_result_node)

    graph.add_edge(START, "extract_user_info")
    graph.add_edge("extract_user_info", "check_missing_info")
    graph.add_conditional_edges(
        "check_missing_info",
        route_by_status,
        {
            "ask_user": "ask_user",
            "finalize_result": "finalize_result",
        },
    )
    graph.add_edge("ask_user", END)
    graph.add_edge("finalize_result", END)

    return graph.compile()


async def run_image_workflow(state: FoodSessionState) -> FoodSessionState:
    current = dict(state)
    current.update(await analyze_image_node(current))
    current.update(check_missing_info_node(current))

    if route_by_status(current) == "finalize_result":
        current.update(finalize_result_node(current))
    else:
        current.update(ask_user_node(current))

    return current


async def run_chat_workflow(state: FoodSessionState) -> FoodSessionState:
    current = dict(state)
    current.update(await extract_user_info_node(current))
    current.update(check_missing_info_node(current))

    if route_by_status(current) == "finalize_result":
        current.update(finalize_result_node(current))
    else:
        current.update(ask_user_node(current))

    return current
