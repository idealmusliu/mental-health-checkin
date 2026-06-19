import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { CheckInForm } from "../CheckInForm";

describe("CheckInForm", () => {
  it("submits the selected mood and trimmed notes", async () => {
    const user = userEvent.setup();
    const onSubmit = jest.fn();
    render(<CheckInForm onSubmit={onSubmit} />);

    await user.click(screen.getByRole("button", { name: /good \(4\)/i }));
    await user.type(screen.getByLabelText(/notes/i), "Felt productive");
    await user.click(screen.getByRole("button", { name: /submit check-in/i }));

    expect(onSubmit).toHaveBeenCalledTimes(1);
    expect(onSubmit).toHaveBeenCalledWith({ mood: 4, notes: "Felt productive" });
  });

  it("submits null notes when left blank", async () => {
    const user = userEvent.setup();
    const onSubmit = jest.fn();
    render(<CheckInForm onSubmit={onSubmit} />);

    await user.click(screen.getByRole("button", { name: /great \(5\)/i }));
    await user.click(screen.getByRole("button", { name: /submit check-in/i }));

    expect(onSubmit).toHaveBeenCalledWith({ mood: 5, notes: null });
  });

  it("blocks submission and shows an error until a mood is chosen", async () => {
    const user = userEvent.setup();
    const onSubmit = jest.fn();
    render(<CheckInForm onSubmit={onSubmit} />);

    await user.click(screen.getByRole("button", { name: /submit check-in/i }));

    expect(onSubmit).not.toHaveBeenCalled();
    expect(screen.getByRole("alert")).toHaveTextContent(/select a mood/i);
  });

  it("pre-fills fields when editing an existing check-in", () => {
    render(
      <CheckInForm onSubmit={jest.fn()} initialMood={2} initialNotes="Earlier note" submitLabel="Save changes" />,
    );

    expect(screen.getByRole("button", { name: /low \(2\)/i })).toHaveAttribute("aria-pressed", "true");
    expect(screen.getByLabelText(/notes/i)).toHaveValue("Earlier note");
    expect(screen.getByRole("button", { name: /save changes/i })).toBeInTheDocument();
  });
});
