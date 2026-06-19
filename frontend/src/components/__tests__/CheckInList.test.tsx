import { render, screen } from "@testing-library/react";
import { CheckInList } from "../CheckInList";
import type { CheckIn } from "@/lib/types";

// next/link needs no router for a unit render; stub it to a plain anchor.
jest.mock("next/link", () => ({
  __esModule: true,
  default: ({ children, href }: { children: React.ReactNode; href: string }) => (
    <a href={href}>{children}</a>
  ),
}));

const items: CheckIn[] = [
  {
    id: "11111111-1111-1111-1111-111111111111",
    userId: "u1",
    userName: "Bob Employee",
    mood: 4,
    notes: "Good day",
    createdAt: "2026-06-10T09:00:00Z",
    updatedAt: null,
  },
  {
    id: "22222222-2222-2222-2222-222222222222",
    userId: "u2",
    userName: "Carol Employee",
    mood: 2,
    notes: null,
    createdAt: "2026-06-11T09:00:00Z",
    updatedAt: null,
  },
];

describe("CheckInList", () => {
  it("renders a row per check-in with mood and author", () => {
    render(<CheckInList items={items} showUser />);

    expect(screen.getByText("Bob Employee")).toBeInTheDocument();
    expect(screen.getByText("Carol Employee")).toBeInTheDocument();
    expect(screen.getByText(/good \(4\)/i)).toBeInTheDocument();
    expect(screen.getByText(/low \(2\)/i)).toBeInTheDocument();
    expect(screen.getByText("Good day")).toBeInTheDocument();
  });

  it("links each row to its detail page", () => {
    render(<CheckInList items={items} />);

    const firstLink = screen.getAllByRole("link")[0];
    expect(firstLink).toHaveAttribute("href", "/checkins/11111111-1111-1111-1111-111111111111");
  });

  it("shows an empty state when there are no check-ins", () => {
    render(<CheckInList items={[]} />);
    expect(screen.getByText(/no check-ins found/i)).toBeInTheDocument();
  });
});
